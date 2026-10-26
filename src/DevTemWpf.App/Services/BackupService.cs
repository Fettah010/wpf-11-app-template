using System.IO;
using System.IO.Compression;

namespace DevTemWpf.App.Services;

/// <summary>
/// Why a backup/restore operation failed (protocol: typed failure results).
/// </summary>
public enum BackupFailure
{
    None = 0,
    MissingManifest,
    UnsupportedVersion,
    CorruptArchive,
    DuplicateEntry,
    TooLarge,
    PathEscape,
}

/// <summary>
/// Thrown for validation/limits failures on backup or restore. Derives from
/// <see cref="IOException"/> so callers that already handle file I/O keep
/// working.
/// </summary>
public sealed class BackupException : IOException
{
    public BackupException(BackupFailure reason, string message)
        : base(message)
        => Reason = reason;

    public BackupFailure Reason { get; }
}

/// <summary>
/// Zip-file backup/restore (BCL only). Writes are atomic (temp + move) and
/// restores are bounded and validated: entry count / total size limits,
/// duplicate rejection, forward-version rejection, and path-escape
/// containment (zip-slip). A settings-only backup is valid; the database is
/// optional.
/// </summary>
public sealed class BackupService : IBackupService
{
    /// <summary>Maximum number of entries accepted on restore.</summary>
    public const int MaxEntries = 64;

    /// <summary>Maximum total uncompressed bytes accepted on restore (512 MiB).</summary>
    public const long MaxUncompressedBytes = 512L * 1024 * 1024;

    private static readonly Version? CurrentVersion =
        typeof(BackupService).Assembly.GetName().Version;

    private readonly string _settingsPath;
    private readonly string _dbPath;

    public BackupService(string? settingsPath = null, string? dbPath = null)
    {
        _settingsPath = settingsPath ?? AppPaths.SettingsPath;
        _dbPath = dbPath ?? AppPaths.DbPath;
    }

    public string CreateBackup(string? destinationDir = null)
    {
        // Pooled SQLite connections hold the file: release them first.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        var dir = destinationDir ?? AppPaths.BackupsDir;
        Directory.CreateDirectory(dir);
        var zipPath = Path.Combine(dir, $"backup-{DateTime.Now:yyyyMMdd-HHmmss}.zip");

        // Atomic: build in a sibling temp file, then move into place, so an
        // interrupted write never leaves a half-written backup at zipPath.
        var tmpPath = zipPath + ".tmp";
        try
        {
            using (var zip = ZipFile.Open(tmpPath, ZipArchiveMode.Create))
            {
                if (File.Exists(_settingsPath))
                {
                    zip.CreateEntryFromFile(_settingsPath, "settings.json");
                }

                foreach (var file in DatabaseFiles())
                {
                    zip.CreateEntryFromFile(file, Path.GetFileName(file));
                }

                zip.CreateEntry("versions.txt").WriteVersions();
            }

            File.Move(tmpPath, zipPath, overwrite: true);
        }
        catch
        {
            TryDelete(tmpPath);
            throw;
        }

        return zipPath;
    }

    public void RestoreBackup(string zipPath)
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        ZipArchive zip;
        try
        {
            zip = ZipFile.OpenRead(zipPath);
        }
        catch (InvalidDataException)
        {
            throw new BackupException(BackupFailure.CorruptArchive, "The backup file is not a valid zip archive.");
        }

        using (zip)
        {
            ValidateArchive(zip);

            var settingsDir = Path.GetDirectoryName(_settingsPath)!;
            var dbDir = Path.GetDirectoryName(_dbPath)!;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in zip.Entries)
            {
                string baseDir;
                string target;
                if (string.Equals(entry.Name, "settings.json", StringComparison.OrdinalIgnoreCase))
                {
                    baseDir = settingsDir;
                    target = _settingsPath;
                }
                else if (entry.Name.StartsWith("app.db", StringComparison.OrdinalIgnoreCase))
                {
                    baseDir = dbDir;
                    target = Path.Combine(dbDir, entry.Name);
                }
                else
                {
                    continue;
                }

                if (!seen.Add(entry.Name))
                {
                    throw new BackupException(
                        BackupFailure.DuplicateEntry,
                        $"The backup contains duplicate '{entry.Name}' entries.");
                }

                EnsureContained(baseDir, target);
                Directory.CreateDirectory(baseDir);
                entry.ExtractToFile(target, overwrite: true);
            }
        }
    }

    private static void ValidateArchive(ZipArchive zip)
    {
        if (zip.Entries.Count > MaxEntries)
        {
            throw new BackupException(
                BackupFailure.TooLarge,
                $"The backup has too many entries ({zip.Entries.Count} > {MaxEntries}).");
        }

        long total = 0;
        foreach (var entry in zip.Entries)
        {
            total += entry.Length;
            if (total > MaxUncompressedBytes)
            {
                throw new BackupException(
                    BackupFailure.TooLarge,
                    "The backup is larger than the supported size limit.");
            }
        }

        if (zip.GetEntry("settings.json") is null)
        {
            throw new BackupException(
                BackupFailure.MissingManifest,
                "Not a DevTemWpf backup: settings.json is missing.");
        }

        ValidateVersion(zip);
    }

    private static void ValidateVersion(ZipArchive zip)
    {
        var versions = zip.GetEntry("versions.txt");
        if (versions is null || CurrentVersion is null)
        {
            return;
        }

        using var reader = new StreamReader(versions.Open());
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line.StartsWith("App:", StringComparison.OrdinalIgnoreCase)
                && Version.TryParse(line[4..].Trim(), out var backupVersion)
                && backupVersion > CurrentVersion)
            {
                throw new BackupException(
                    BackupFailure.UnsupportedVersion,
                    $"The backup was created by a newer app version ({backupVersion}); it cannot be restored here.");
            }
        }
    }

    private static void EnsureContained(string dir, string path)
    {
        string fullDir;
        string fullPath;
        try
        {
            fullDir = Path.GetFullPath(dir);
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception)
        {
            throw new BackupException(BackupFailure.PathEscape, "The backup contains an invalid file path.");
        }

        var prefix = fullDir.EndsWith(Path.DirectorySeparatorChar)
            ? fullDir
            : fullDir + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new BackupException(
                BackupFailure.PathEscape,
                "The backup tried to write outside the application data folder.");
        }
    }

    private IEnumerable<string> DatabaseFiles()
    {
        var dir = Path.GetDirectoryName(_dbPath)!;
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var file = _dbPath + suffix;
            if (File.Exists(file) && Path.GetDirectoryName(file) == dir)
            {
                yield return file;
            }
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best effort: a leftover .tmp is harmless and gets overwritten.
        }
    }
}

file static class ZipEntryExtensions
{
    internal static void WriteVersions(this ZipArchiveEntry entry)
    {
        using var writer = new StreamWriter(entry.Open());
        writer.WriteLine($"App: {typeof(BackupService).Assembly.GetName().Version}");
        writer.WriteLine($"OS: {Environment.OSVersion.VersionString}");
        writer.WriteLine($"Created (UTC): {DateTime.UtcNow:o}");
    }
}
