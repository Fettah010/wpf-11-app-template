namespace DevTemWpf.App.Services;

/// <summary>Update check result (engine-agnostic).</summary>
public sealed record UpdateCheck(string Version, long SizeBytes);

/// <summary>
/// Thin seam over the update engine so states are unit-testable without
/// releases, feeds, or installs.
/// </summary>
public interface IUpdateEngine
{
    Task<UpdateCheck?> CheckAsync(string feedUrl, CancellationToken cancellationToken);

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

    Task CheckAsync(CancellationToken cancellationToken = default);

    Task DownloadAndRestartAsync(CancellationToken cancellationToken = default);
}
