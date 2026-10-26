using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Resources;
using DevTemWpf.App.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Diagnostics: logs, support bundle, backup/restore, updates, HTTP probe.
/// File pickers live in code-behind; everything here takes explicit paths and
/// is unit-testable (ADR-0011).
/// </summary>
public sealed partial class DiagnosticsViewModel : ObservableObject, INavigationAware
{
    private readonly ILogger<DiagnosticsViewModel> _log;
    private readonly ISettingsService _settings;
    private readonly IBackupService _backup;
    private readonly StatusClient _http;
    private readonly HttpOptions _httpOptions;
    private readonly UpdateOptions _updateOptions;
    private readonly string _logsDir;
    private readonly string _settingsPath;

    public DiagnosticsViewModel(
        ILogger<DiagnosticsViewModel> log,
        ISettingsService settings,
        IUpdateService update,
        IBackupService backup,
        StatusClient http,
        IOptions<HttpOptions> httpOptions,
        IOptions<UpdateOptions> updateOptions,
        string? logsDir = null,
        string? settingsPath = null)
    {
        _log = log;
        _settings = settings;
        Update = update;
        _backup = backup;
        _http = http;
        _httpOptions = httpOptions.Value;
        _updateOptions = updateOptions.Value;
        _settingsPath = settingsPath ?? AppPaths.SettingsPath;
        _logsDir = logsDir ?? AppPaths.LogsDir;
        ChannelOptions = [UpdateChannel.Stable, UpdateChannel.Beta];
        PathsText = $"Settings: {AppPaths.SettingsPath}\nDatabase: {AppPaths.DbPath}\nLogs: {AppPaths.LogsDir}\nBackups: {AppPaths.BackupsDir}";
        VersionsText = $"App: {GetType().Assembly.GetName().Version}\nRuntime: {Environment.Version}\nOS: {Environment.OSVersion.VersionString}";
        if (update is System.ComponentModel.INotifyPropertyChanged observable)
        {
            observable.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(IUpdateService.AvailableVersion) or nameof(IUpdateService.Status))
                {
                    RefreshUpdateVersions();
                }
            };
        }

        RefreshUpdateVersions();
        RefreshLogs();
    }

    public IUpdateService Update { get; }

    public IReadOnlyList<UpdateChannel> ChannelOptions { get; }

    public ObservableCollection<string> LogLines { get; } = [];

    [ObservableProperty]
    private string logFilter = string.Empty;

    [ObservableProperty]
    private string logStatus = string.Empty;

    [ObservableProperty]
    private string backupStatus = string.Empty;

    [ObservableProperty]
    private string probeResult = string.Empty;

    [ObservableProperty]
    private UpdateChannel selectedChannel = UpdateChannel.Stable;

    [ObservableProperty]
    private string feedUrlText = string.Empty;

    /// <summary>Installed vs available version line (1.6.0).</summary>
    [ObservableProperty]
    private string updateVersionsText = string.Empty;

    /// <summary>Opt-in switch required before an older version may be applied.</summary>
    [ObservableProperty]
    private bool allowDowngrade;

    public string PathsText { get; }

    public string VersionsText { get; }

    public string HttpBaseUrl => string.IsNullOrWhiteSpace(_httpOptions.BaseUrl)
        ? "(not configured)"
        : _httpOptions.BaseUrl;

    public Task OnNavigatedToAsync()
    {
        SelectedChannel = _settings.Current.UpdateChannel;
        RefreshFeedText();
        RefreshUpdateVersions();
        RefreshLogs();
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
    [RelayCommand]
    public void RefreshLogs()
    {
        LogLines.Clear();
        try
        {
            if (!Directory.Exists(_logsDir))
            {
                LogStatus = Strings.Dia_NoLogs;;
                return;
            }

            var files = Directory.GetFiles(_logsDir, "app-*.log")
                .OrderDescending()
                .Take(3)
                .ToList();
            var taken = 0;
            foreach (var file in files)
            {
                foreach (var line in File.ReadLines(file).Reverse().Take(300 - taken))
                {
                    if (MatchesFilter(line))
                    {
                        LogLines.Insert(0, line);
                        if (++taken >= 300)
                        {
                            break;
                        }
                    }
                }
            }

            LogStatus = string.Format(Strings.Dia_LogCountFormat, taken, files.Count);
            _log.LogDebug("Refreshed log view ({Count} lines).", taken);
        }
        catch (Exception ex)
        {
            LogStatus = string.Format(Strings.Dia_CannotReadLogs, ex.Message);
            _log.LogWarning(ex, "Log refresh failed.");
        }
    }

    public void ExportLog(string destinationPath)
    {
        var newest = NewestLog();
        if (newest is null)
        {
            LogStatus = Strings.Dia_NoLogsExport;
            return;
        }

        File.Copy(newest, destinationPath, overwrite: true);
        LogStatus = string.Format(Strings.Dia_Exported, Path.GetFileName(newest));
        _log.LogInformation("Exported log to {Path}.", destinationPath);
    }

    public void CreateSupportBundle(string destinationPath)
    {
        using var zip = ZipFile.Open(destinationPath, ZipArchiveMode.Create);
        foreach (var file in Directory.Exists(_logsDir)
                     ? Directory.GetFiles(_logsDir, "app-*.log").OrderDescending().Take(5)
                     : [])
        {
            zip.CreateEntryFromFile(file, $"logs/{Path.GetFileName(file)}");
        }

        if (File.Exists(_settingsPath))
        {
            zip.CreateEntryFromFile(_settingsPath, "settings.json");
        }

        var versions = zip.CreateEntry("versions.txt");
        using (var writer = new StreamWriter(versions.Open()))
        {
            writer.Write(VersionsText);
        }

        LogStatus = string.Format(Strings.Dia_BundleWritten, Path.GetFileName(destinationPath));
        _log.LogInformation("Created support bundle at {Path}.", destinationPath);
    }

    [RelayCommand]
    public void BackupNow(string? destinationDir = null)
    {
        try
        {
            var path = _backup.CreateBackup(destinationDir);
            BackupStatus = string.Format(Strings.Dia_BackupWritten, Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            BackupStatus = string.Format(Strings.Dia_BackupFailed, ex.Message);
            _log.LogWarning(ex, "Backup failed.");
        }
    }

    public void RestoreBackup(string zipPath)
    {
        try
        {
            _backup.RestoreBackup(zipPath);
            BackupStatus = Strings.Dia_RestoreDone;
        }
        catch (Exception ex)
        {
            BackupStatus = string.Format(Strings.Dia_RestoreFailedFormat, ex.Message);
            _log.LogWarning(ex, "Restore failed.");
        }
    }

    [RelayCommand]
    private async Task CheckUpdatesAsync(CancellationToken cancellationToken)
    {
        await Update.CheckAsync(cancellationToken, AllowDowngrade).ConfigureAwait(true);
        RefreshFeedText();
        RefreshUpdateVersions();
    }

    [RelayCommand]
    private async Task DownloadUpdatesAsync(CancellationToken cancellationToken)
        => await Update.DownloadAndRestartAsync(cancellationToken).ConfigureAwait(true);

    [RelayCommand]
    private Task RetryUpdatesAsync(CancellationToken cancellationToken) => Update.RetryAsync(cancellationToken);

    [RelayCommand]
    private void CancelUpdates() => Update.Cancel();

    [RelayCommand]
    private Task RestartUpdatesAsync() => Update.RestartNowAsync();

    [RelayCommand]
    private async Task ProbeHttpAsync(CancellationToken cancellationToken)
    {
        var (ok, detail) = await _http.GetStatusAsync(cancellationToken).ConfigureAwait(true);
        ProbeResult = ok ? $"Reachable — {detail}" : detail;
        _log.LogInformation("HTTP probe: {Result}", ProbeResult);
    }

    partial void OnLogFilterChanged(string value) => RefreshLogs();

    partial void OnSelectedChannelChanged(UpdateChannel value)
    {
        _ = _settings.UpdateAsync(s => s with { UpdateChannel = value });
        RefreshFeedText(value);
    }

    private void RefreshFeedText(UpdateChannel? channel = null)
    {
        var selected = channel ?? _settings.Current.UpdateChannel;
        var url = selected == UpdateChannel.Beta && !string.IsNullOrWhiteSpace(_updateOptions.BetaFeedUrl)
            ? _updateOptions.BetaFeedUrl
            : _updateOptions.StableFeedUrl;
        FeedUrlText = string.IsNullOrWhiteSpace(url) ? "(updates not configured)" : url.Trim();
    }

    private void RefreshUpdateVersions()
        => UpdateVersionsText = string.Format(
            Strings.Dia_AppVersionsFormat,
            Update.CurrentVersion,
            Update.AvailableVersion ?? "—");

    private bool MatchesFilter(string line)
        => string.IsNullOrWhiteSpace(LogFilter)
            || line.Contains(LogFilter, StringComparison.OrdinalIgnoreCase);

    private string? NewestLog()
        => Directory.Exists(_logsDir)
            ? Directory.GetFiles(_logsDir, "app-*.log").OrderDescending().FirstOrDefault()
            : null;
}
