using System.IO.Compression;
using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class BackupServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void RoundTrip_PreservesSettingsAndDatabase()
    {
        var home = Path.Combine(_dir, "home");
        Directory.CreateDirectory(home);
        var settings = Path.Combine(home, "settings.json");
        var db = Path.Combine(home, "app.db");
        File.WriteAllText(settings, """{"Theme":1}""");
        new FavoritesStore(db).ToggleFavorite(9);

        var zip = new BackupService(settings, db).CreateBackup(Path.Combine(_dir, "out"));
        Assert.True(File.Exists(zip));

        File.WriteAllText(settings, """{"Theme":2}""");
        new BackupService(settings, db).RestoreBackup(zip);

        Assert.Equal("""{"Theme":1}""", File.ReadAllText(settings));
        Assert.True(new FavoritesStore(db).IsFavorite(9));
    }

    [Fact]
    public void Restore_RejectsNonBackupZip()
    {
        var zip = Path.Combine(_dir, "bogus.zip");
        Directory.CreateDirectory(_dir);
        using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("readme.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("nope");
        }

        var ex = Assert.Throws<BackupException>(() =>
            new BackupService(Path.Combine(_dir, "s.json"), Path.Combine(_dir, "a.db"))
                .RestoreBackup(zip));
        Assert.Equal(BackupFailure.MissingManifest, ex.Reason);
    }

    [Fact]
    public void Restore_RejectsCorruptArchive()
    {
        var bad = Path.Combine(_dir, "corrupt.zip");
        Directory.CreateDirectory(_dir);
        File.WriteAllBytes(bad, [0x00, 0x01, 0x02, 0x03, 0x04, 0x05]);

        var ex = Assert.Throws<BackupException>(() =>
            new BackupService(Path.Combine(_dir, "s.json"), Path.Combine(_dir, "a.db"))
                .RestoreBackup(bad));
        Assert.Equal(BackupFailure.CorruptArchive, ex.Reason);
    }

    [Fact]
    public void Restore_RejectsDuplicateEntries()
    {
        var zip = WriteZip("dup.zip", a =>
        {
            AddText(a, "settings.json", """{"Theme":1}""");
            AddText(a, "settings.json", """{"Theme":3}""");
        });

        var ex = Assert.Throws<BackupException>(() => Restore(zip));
        Assert.Equal(BackupFailure.DuplicateEntry, ex.Reason);
    }

    [Fact]
    public void Restore_RejectsUnsupportedVersion()
    {
        var zip = WriteZip("future.zip", a =>
        {
            AddText(a, "settings.json", """{"Theme":1}""");
            AddText(a, "versions.txt", "App: 99.0.0.0\n");
        });

        var ex = Assert.Throws<BackupException>(() => Restore(zip));
        Assert.Equal(BackupFailure.UnsupportedVersion, ex.Reason);
    }

    [Fact]
    public void Restore_RejectsOversizedArchive()
    {
        var zip = WriteZip("big.zip", a =>
        {
            AddText(a, "settings.json", """{"Theme":1}""");
            for (var i = 0; i < BackupService.MaxEntries + 5; i++)
            {
                AddText(a, $"junk-{i}.txt", "x");
            }
        });

        var ex = Assert.Throws<BackupException>(() => Restore(zip));
        Assert.Equal(BackupFailure.TooLarge, ex.Reason);
    }

    [Fact]
    public void Restore_IgnoresTraversalEntry_NoEscape()
    {
        var zip = WriteZip("escape.zip", a =>
        {
            AddText(a, "settings.json", """{"Theme":1}""");
            // The engine's Name strips both separators, so a traversal name
            // collapses to its leaf and is ignored (defense in depth: the
            // containment check would also reject it). Either way nothing may
            // be written outside the data folder and valid entries still land.
            AddText(a, "app.db/../../evil.db", "x");
            AddText(a, "app.db", "not-a-real-db");
        });

        var settings = Path.Combine(_dir, "home", "settings.json");
        var db = Path.Combine(_dir, "home", "app.db");
        new BackupService(settings, db).RestoreBackup(zip);

        Assert.False(File.Exists(Path.Combine(_dir, "evil.db")));
        Assert.Equal("""{"Theme":1}""", File.ReadAllText(settings));
        Assert.Equal("not-a-real-db", File.ReadAllText(db));
    }

    [Fact]
    public void Restore_AllowsSettingsOnlyBackup()
    {
        var zip = WriteZip("settings-only.zip", a =>
            AddText(a, "settings.json", """{"Theme":4}"""));

        var settings = Path.Combine(_dir, "new", "settings.json");
        var db = Path.Combine(_dir, "new", "app.db");
        new BackupService(settings, db).RestoreBackup(zip);

        Assert.Equal("""{"Theme":4}""", File.ReadAllText(settings));
    }

    [Fact]
    public void Restore_ToNewLocation_RoundTrips()
    {
        var src = Path.Combine(_dir, "src");
        Directory.CreateDirectory(src);
        var srcSettings = Path.Combine(src, "settings.json");
        var srcDb = Path.Combine(src, "app.db");
        File.WriteAllText(srcSettings, """{"Theme":7}""");
        new FavoritesStore(srcDb).ToggleFavorite(3);

        var zip = new BackupService(srcSettings, srcDb).CreateBackup(Path.Combine(_dir, "out"));

        var dst = Path.Combine(_dir, "dst");
        var dstSettings = Path.Combine(dst, "settings.json");
        var dstDb = Path.Combine(dst, "app.db");
        new BackupService(dstSettings, dstDb).RestoreBackup(zip);

        Assert.Equal("""{"Theme":7}""", File.ReadAllText(dstSettings));
        Assert.True(new FavoritesStore(dstDb).IsFavorite(3));
    }

    [Fact]
    public void CreateBackup_IsAtomic_AndLeavesNoTemp()
    {
        var home = Path.Combine(_dir, "home");
        Directory.CreateDirectory(home);
        var settings = Path.Combine(home, "settings.json");
        File.WriteAllText(settings, """{"Theme":1}""");

        var dir = Path.Combine(_dir, "out");
        var zip = new BackupService(settings, Path.Combine(home, "app.db")).CreateBackup(dir);

        Assert.True(File.Exists(zip));
        Assert.Empty(Directory.GetFiles(dir, "*.tmp"));
    }

    private void Restore(string zip)
        => new BackupService(Path.Combine(_dir, "home", "settings.json"), Path.Combine(_dir, "home", "app.db"))
            .RestoreBackup(zip);

    private string WriteZip(string name, Action<ZipArchive> build)
    {
        Directory.CreateDirectory(_dir);
        var path = Path.Combine(_dir, name);
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        build(archive);
        return path;
    }

    private static void AddText(ZipArchive archive, string name, string text)
    {
        var entry = archive.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open());
        writer.Write(text);
    }
}
