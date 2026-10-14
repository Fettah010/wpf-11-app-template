using Velopack;
using Velopack.Sources;

namespace DevTemWpf.App.Services;

/// <summary>Production <see cref="IUpdateEngine"/> over Velopack (MIT).</summary>
public sealed class VelopackUpdateEngine : IUpdateEngine
{
    public async Task<UpdateCheck?> CheckAsync(string feedUrl, CancellationToken cancellationToken)
    {
        var manager = new UpdateManager(new SimpleWebSource(feedUrl));
        var update = await manager.CheckForUpdatesAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return update is null
            ? null
            : new UpdateCheck(update.TargetFullRelease.Version.ToString(), update.TargetFullRelease.Size);
    }

    public async Task DownloadAsync(string feedUrl, UpdateCheck update, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var manager = new UpdateManager(new SimpleWebSource(feedUrl));
        var info = await manager.CheckForUpdatesAsync().ConfigureAwait(false);
        if (info is null)
        {
            throw new InvalidOperationException("The update is no longer available.");
        }

        await manager.DownloadUpdatesAsync(info, ProgressAction).ConfigureAwait(false);
        return;

        void ProgressAction(int percent) => progress.Report(percent / 100.0);
    }

    public void ApplyAndRestart(string feedUrl, UpdateCheck update)
    {
        var manager = new UpdateManager(new SimpleWebSource(feedUrl));
        var pending = manager.CheckForUpdatesAsync().GetAwaiter().GetResult();
        if (pending is null)
        {
            throw new InvalidOperationException("The update is no longer available.");
        }

        manager.ApplyUpdatesAndRestart(pending);
    }
}
