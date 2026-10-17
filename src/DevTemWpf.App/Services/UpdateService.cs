using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Resources;
using Microsoft.Extensions.Options;

namespace DevTemWpf.App.Services;

/// <summary>
/// Explicit update flow (ADR-0011; extended in 1.6.0). No automatic checks:
/// every network use follows a user action, and every failure becomes status
/// text. Supports current-vs-available display, channel selection (with an
/// explicit downgrade opt-in), retry, cancel, and a restart prompt. Delta vs
/// full is decided by the engine (Velopack) and surfaced, never reimplemented.
/// </summary>
public sealed partial class UpdateService : ObservableObject, IUpdateService
{
    private readonly ISettingsService _settings;
    private readonly IOptions<UpdateOptions> _options;
    private readonly Func<IUpdateEngine> _engineFactory;
    private readonly string _currentVersion;
    private UpdateCheck? _pending;
    private UpdateOperation _lastOperation = UpdateOperation.None;
    private bool _lastAllowDowngrade;
    private CancellationTokenSource? _cts;

    public UpdateService(
        ISettingsService settings,
        IOptions<UpdateOptions> options,
        Func<IUpdateEngine>? engineFactory = null,
        string? currentVersion = null)
    {
        _settings = settings;
        _options = options;
        _engineFactory = engineFactory ?? (() => new VelopackUpdateEngine());
        _currentVersion = currentVersion
            ?? typeof(UpdateService).Assembly.GetName().Version?.ToString(3)
            ?? "0.0.0";
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    [NotifyPropertyChangedFor(nameof(AvailableVersion))]
    [NotifyPropertyChangedFor(nameof(IsDowngrade))]
    private UpdateStatus status = UpdateStatus.Idle;

    [ObservableProperty]
    private string detail = string.Empty;

    [ObservableProperty]
    private double progress;

    public string EffectiveFeedUrl
    {
        get
        {
            var urls = _options.Value;
            var url = _settings.Current.UpdateChannel == UpdateChannel.Beta && !string.IsNullOrWhiteSpace(urls.BetaFeedUrl)
                ? urls.BetaFeedUrl
                : urls.StableFeedUrl;
            return url.Trim();
        }
    }

    public string CurrentVersion => _currentVersion;

    public string? AvailableVersion => _pending?.Version;

    public bool IsBusy => Status is UpdateStatus.Checking or UpdateStatus.Downloading;

    public bool IsDowngrade => _pending is not null && IsLower(_pending.Version, _currentVersion);

    public async Task CheckAsync(CancellationToken cancellationToken = default, bool allowDowngrade = false)
    {
        var feed = EffectiveFeedUrl;
        if (string.IsNullOrEmpty(feed))
        {
            Status = UpdateStatus.Disabled;
            Detail = Strings.Update_Disabled;
            return;
        }

        _lastOperation = UpdateOperation.Check;
        _lastAllowDowngrade = allowDowngrade;
        var token = BeginOperation(cancellationToken);

        Status = UpdateStatus.Checking;
        Detail = string.Format(Strings.Update_CheckingFormat, feed);
        _pending = null;
        try
        {
            var found = await _engineFactory().CheckAsync(feed, allowDowngrade, token).ConfigureAwait(false);
            if (token.IsCancellationRequested)
            {
                Status = UpdateStatus.Idle;
                Detail = Strings.Update_Cancelled;
                return;
            }

            if (found is null)
            {
                Status = UpdateStatus.UpToDate;
                Detail = Strings.Update_UpToDate;
            }
            else
            {
                _pending = found;
                Status = UpdateStatus.Available;
                Detail = Describe(found);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            Status = UpdateStatus.Idle;
            Detail = Strings.Update_Cancelled;
        }
        catch (Exception ex)
        {
            Status = UpdateStatus.Failed;
            Detail = string.Format(Strings.Update_FailedFormat, ex.Message);
        }
    }

    public async Task DownloadAsync(CancellationToken cancellationToken = default)
    {
        if (_pending is null)
        {
            Detail = Strings.Update_NothingToDownload;
            return;
        }

        _lastOperation = UpdateOperation.Download;
        var feed = EffectiveFeedUrl;
        var token = BeginOperation(cancellationToken);

        Status = UpdateStatus.Downloading;
        Progress = 0;
        try
        {
            var progress = new Progress<double>(p => Progress = p);
            await _engineFactory().DownloadAsync(feed, _pending, progress, token).ConfigureAwait(false);
            Progress = 1.0;
            Status = UpdateStatus.ReadyToRestart;
            Detail = string.Format(Strings.Update_ReadyFormat, _pending.Version) + " " + Strings.Update_RestartPrompt;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            Status = UpdateStatus.Idle;
            Detail = Strings.Update_DownloadCancelled;
        }
        catch (Exception ex)
        {
            Status = UpdateStatus.Failed;
            Detail = string.Format(Strings.Update_DownloadFailedFormat, ex.Message);
        }
    }

    public Task RestartNowAsync()
    {
        if (_pending is null || Status != UpdateStatus.ReadyToRestart)
        {
            return Task.CompletedTask;
        }

        _engineFactory().ApplyAndRestart(EffectiveFeedUrl, _pending);
        return Task.CompletedTask;
    }

    public async Task DownloadAndRestartAsync(CancellationToken cancellationToken = default)
    {
        await DownloadAsync(cancellationToken).ConfigureAwait(false);
        if (Status == UpdateStatus.ReadyToRestart)
        {
            await RestartNowAsync().ConfigureAwait(false);
        }
    }

    public Task RetryAsync(CancellationToken cancellationToken = default)
        => _lastOperation switch
        {
            UpdateOperation.Check => CheckAsync(cancellationToken, _lastAllowDowngrade),
            UpdateOperation.Download => DownloadAsync(cancellationToken),
            _ => Task.CompletedTask,
        };

    public void Cancel() => _cts?.Cancel();

    private string Describe(UpdateCheck check)
    {
        var template = check.IsDelta ? Strings.Update_AvailableDeltaFormat : Strings.Update_AvailableFullFormat;
        var text = string.Format(template, check.Version, check.SizeBytes / 1024);
        return IsLower(check.Version, _currentVersion)
            ? text + " " + string.Format(Strings.Update_DowngradeFormat, check.Version, _currentVersion)
            : text;
    }

    private CancellationToken BeginOperation(CancellationToken external)
    {
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(external);
        return _cts.Token;
    }

    private static bool IsLower(string candidate, string current)
        => Version.TryParse(candidate, out var c)
            && Version.TryParse(current, out var cur)
            && c < cur;

    private enum UpdateOperation
    {
        None,
        Check,
        Download,
    }
}
