using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;

namespace DevTemWpf.App.Services;

/// <summary>
/// Explicit update flow (ADR-0011). No automatic checks: every network use
/// follows a user action, and every failure becomes status text.
/// </summary>
public sealed partial class UpdateService : ObservableObject, IUpdateService
{
    private readonly ISettingsService _settings;
    private readonly IOptions<UpdateOptions> _options;
    private readonly Func<IUpdateEngine> _engineFactory;
    private UpdateCheck? _pending;

    public UpdateService(ISettingsService settings, IOptions<UpdateOptions> options, Func<IUpdateEngine>? engineFactory = null)
    {
        _settings = settings;
        _options = options;
        _engineFactory = engineFactory ?? (() => new VelopackUpdateEngine());
    }

    [ObservableProperty]
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

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        var feed = EffectiveFeedUrl;
        if (string.IsNullOrEmpty(feed))
        {
            Status = UpdateStatus.Disabled;
            Detail = "Updates are not configured (empty feed URL).";
            return;
        }

        Status = UpdateStatus.Checking;
        Detail = $"Checking {feed}…";
        _pending = null;
        try
        {
            var found = await _engineFactory().CheckAsync(feed, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                Status = UpdateStatus.Idle;
                Detail = "Check cancelled.";
                return;
            }

            if (found is null)
            {
                Status = UpdateStatus.UpToDate;
                Detail = "You're up to date.";
            }
            else
            {
                _pending = found;
                Status = UpdateStatus.Available;
                Detail = $"Version {found.Version} available ({found.SizeBytes / 1024} KB).";
            }
        }
        catch (Exception ex)
        {
            Status = UpdateStatus.Failed;
            Detail = $"Check failed: {ex.Message}";
        }
    }

    public async Task DownloadAndRestartAsync(CancellationToken cancellationToken = default)
    {
        if (_pending is null)
        {
            Detail = "Nothing to download — check for updates first.";
            return;
        }

        var feed = EffectiveFeedUrl;
        Status = UpdateStatus.Downloading;
        Progress = 0;
        try
        {
            var progress = new Progress<double>(p => Progress = p);
            await _engineFactory().DownloadAsync(feed, _pending, progress, cancellationToken).ConfigureAwait(false);
            Progress = 1.0;
            Status = UpdateStatus.ReadyToRestart;
            Detail = $"Version {_pending.Version} ready — restarting…";
            _engineFactory().ApplyAndRestart(feed, _pending);
        }
        catch (Exception ex)
        {
            Status = UpdateStatus.Failed;
            Detail = $"Download failed: {ex.Message}";
        }
    }
}
