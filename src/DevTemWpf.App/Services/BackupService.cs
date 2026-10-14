using System.IO;
using System.IO.Compression;

namespace DevTemWpf.App.Services;

/// <summary>Zip-file backup/restore (BCL only).</summary>
public sealed class BackupService : IBackupService
{
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
        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        if (File.Exists(_settingsPath))
        {
            zip.CreateEntryFromFile(_settingsPath, "settings.json");
        }

        foreach (var file in DatabaseFiles())
        {
            zip.CreateEntryFromFile(file, Path.GetFileName(file));
        }

        zip.CreateEntry("versions.txt").WriteVersions();
        return zipPath;
    }

    public void RestoreBackup(string zipPath)
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        using var zip = ZipFile.OpenRead(zipPath);
        if (zip.GetEntry("settings.json") is null)
        {
            throw new InvalidDataException("Not a DevTemWpf backup: settings.json is missing.");
        }

        var settingsDir = Path.GetDirectoryName(_settingsPath)!;
        var dbDir = Path.GetDirectoryName(_dbPath)!;
        Directory.CreateDirectory(settingsDir);
        Directory.CreateDirectory(dbDir);

        foreach (var entry in zip.Entries)
        {
            var target = entry.Name switch
            {
                "settings.json" => _settingsPath,
                var name when name.StartsWith("app.db", StringComparison.OrdinalIgnoreCase)
                    => Path.Combine(dbDir, name),
                _ => null,
            };
            if (target is not null)
            {
                entry.ExtractToFile(target, overwrite: true);
            }
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
