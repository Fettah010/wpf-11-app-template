namespace DevTemWpf.App.Services;

/// <summary>Update check result (engine-agnostic).</summary>
/// <param name="Version">Available version string.</param>
/// <param name="SizeBytes">Full-release size in bytes (upper bound).</param>
/// <param name="IsDelta">True when a delta patch is available for this client.</param>
public sealed record UpdateCheck(string Version, long SizeBytes, bool IsDelta = false);

/// <summary>
/// Thin seam over the update engine so states are unit-testable without
/// releases, feeds, or installs.
/// </summary>
public interface IUpdateEngine
{
    Task<UpdateCheck?> CheckAsync(string feedUrl, bool allowDowngrade, CancellationToken cancellationToken);

    Task DownloadAsync(string feedUrl, UpdateCheck update, IProgress<double> progress, CancellationToken cancellationToken);

    void ApplyAndRestart(string feedUrl, UpdateCheck update);
}

/// <summary>User-facing update flow: explicit checks only, never automatic.</summary>
public interface IUpdateService
{
    UpdateStatus Status { get; }

    string Detail { get; }

    double Progress { get; }

    string EffectiveFeedUrl { get; }

    /// <summary>Currently installed app version.</summary>
    string CurrentVersion { get; }

    /// <summary>Pending available version, or null when none is known.</summary>
    string? AvailableVersion { get; }

    /// <summary>True while a check or download is in flight.</summary>
    bool IsBusy { get; }

    /// <summary>True when the pending update is older than the installed app.</summary>
    bool IsDowngrade { get; }

    Task CheckAsync(CancellationToken cancellationToken = default, bool allowDowngrade = false);

    /// <summary>Downloads the pending update without restarting.</summary>
    Task DownloadAsync(CancellationToken cancellationToken = default);

    /// <summary>Applies a downloaded update and restarts (the restart prompt).</summary>
    Task RestartNowAsync();

    /// <summary>Convenience: download then restart.</summary>
    Task DownloadAndRestartAsync(CancellationToken cancellationToken = default);

    /// <summary>Repeats the last failed check or download.</summary>
    Task RetryAsync(CancellationToken cancellationToken = default);

    /// <summary>Cancels the in-flight check or download.</summary>
    void Cancel();
}
