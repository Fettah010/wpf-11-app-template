using System.IO.Compression;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Unit;

public sealed class DiagnosticsViewModelTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly string _settings = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
        try { File.Delete(_settings); } catch { /* best effort */ }
    }

    private DiagnosticsViewModel Create(string? logsDir = null)
    {
        var settings = new SettingsService(_settings);
        var update = new UpdateService(settings, Options.Create(new UpdateOptions()));
        var backup = new BackupService(
            Path.Combine(_dir, "settings.json"), Path.Combine(_dir, "app.db"));
        var http = new StatusClient(new HttpClient(),
            Options.Create(new HttpOptions { BaseUrl = string.Empty }));
        return new DiagnosticsViewModel(
            NullLogger<DiagnosticsViewModel>.Instance, settings, update, backup, http,
            Options.Create(new HttpOptions { BaseUrl = string.Empty }),
            Options.Create(new UpdateOptions()),
            logsDir ?? Path.Combine(_dir, "logs"),
            Path.Combine(_dir, "settings.json"));
    }

    [Fact]
    public void RefreshLogs_ReadsAndFilters()
    {
        var logs = Path.Combine(_dir, "logs");
        Directory.CreateDirectory(logs);
        File.WriteAllLines(Path.Combine(logs, "app-20261014.log"),
            ["2026-10-14 [INF] Startup Starting", "2026-10-14 [WRN] Net Retrying", "2026-10-14 [INF] Startup Ready"]);

        var vm = Create(logs);
        Assert.Equal(3, vm.LogLines.Count);

        vm.LogFilter = "wrn";
        Assert.Single(vm.LogLines);
    }

    [Fact]
    public void SupportBundle_ContainsLogsAndSettings()
    {
        var logs = Path.Combine(_dir, "logs");
        Directory.CreateDirectory(logs);
        File.WriteAllText(Path.Combine(logs, "app-20261014.log"), "line");
        File.WriteAllText(Path.Combine(_dir, "settings.json"), "{}");

        var vm = Create(logs);
        var dest = Path.Combine(_dir, "bundle.zip");

        vm.CreateSupportBundle(dest);

        using var zip = ZipFile.OpenRead(dest);
        Assert.Contains(zip.Entries, e => e.FullName.StartsWith("logs/"));
        Assert.Contains(zip.Entries, e => e.FullName == "settings.json");
    }
}
