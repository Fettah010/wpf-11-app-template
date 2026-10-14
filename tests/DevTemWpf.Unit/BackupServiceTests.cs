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

        Assert.Throws<InvalidDataException>(() =>
            new BackupService(Path.Combine(_dir, "s.json"), Path.Combine(_dir, "a.db"))
                .RestoreBackup(zip));
    }
}
