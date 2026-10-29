using DevTemWpf.App.Services;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Unit;

public sealed class UpdateServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    private sealed class QueuedSynchronizationContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback Callback, object? State)> _callbacks = new();

        public override void Post(SendOrPostCallback callback, object? state)
            => _callbacks.Enqueue((callback, state));

        public void Drain()
        {
            while (_callbacks.Count > 0)
            {
                var (callback, state) = _callbacks.Dequeue();
                callback(state);
            }
        }
    }

    public void Dispose()
    {
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    private sealed class FakeEngine : IUpdateEngine
    {
        public UpdateCheck? CheckResult { get; set; }
        public Exception? CheckError { get; set; }
        public Exception? DownloadError { get; set; }
        public bool Restarted { get; private set; }
        public bool? LastAllowDowngrade { get; private set; }
        public TaskCompletionSource? Gate { get; set; }

        public async Task<UpdateCheck?> CheckAsync(string feedUrl, bool allowDowngrade, CancellationToken cancellationToken)
        {
            LastAllowDowngrade = allowDowngrade;
            if (Gate is not null)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            }

            if (CheckError is not null)
            {
                throw CheckError;
            }

            return CheckResult;
        }

        public Task DownloadAsync(string feedUrl, UpdateCheck update, IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (DownloadError is not null)
            {
                return Task.FromException(DownloadError);
            }

            progress.Report(0.5);
            progress.Report(1.0);
            return Task.CompletedTask;
        }

        public void ApplyAndRestart(string feedUrl, UpdateCheck update) => Restarted = true;
    }

    private async Task<(UpdateService Service, FakeEngine Engine)> CreateAsync(
        UpdateOptions? options = null, ShellSettings? seed = null, string currentVersion = "1.0.0")
    {
        var settings = new SettingsService(_path);
        await settings.UpdateAsync(_ => seed ?? new ShellSettings(), TestContext.Current.CancellationToken);
        var engine = new FakeEngine();
        return (new UpdateService(settings, Options.Create(options ?? new UpdateOptions()), () => engine, currentVersion), engine);
    }

    [Fact]
    public async Task EmptyFeeds_AreDisabled()
    {
        var (service, _) = await CreateAsync();

        await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.Equal(UpdateStatus.Disabled, service.Status);
    }

    [Fact]
    public async Task AvailableUpdate_FlowsToRestart()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckResult = new UpdateCheck("9.9.9", 2048);

        await service.CheckAsync(TestContext.Current.CancellationToken);
        Assert.Equal(UpdateStatus.Available, service.Status);
        Assert.Contains("9.9.9", service.Detail);

        var previousContext = SynchronizationContext.Current;
        var progressContext = new QueuedSynchronizationContext();
        Task downloadTask;
        try
        {
            SynchronizationContext.SetSynchronizationContext(progressContext);
            downloadTask = service.DownloadAndRestartAsync(TestContext.Current.CancellationToken);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }

        await downloadTask;
        progressContext.Drain();
        Assert.Equal(UpdateStatus.ReadyToRestart, service.Status);
        Assert.True(engine.Restarted);
        Assert.Equal(1.0, service.Progress);
    }

    [Fact]
    public async Task DownloadWithoutRestart_AwaitsPrompt()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckResult = new UpdateCheck("9.9.9", 2048);
        await service.CheckAsync(TestContext.Current.CancellationToken);

        await service.DownloadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(UpdateStatus.ReadyToRestart, service.Status);
        Assert.False(engine.Restarted);
        Assert.Contains("restart", service.Detail, StringComparison.OrdinalIgnoreCase);

        await service.RestartNowAsync();
        Assert.True(engine.Restarted);
    }

    [Fact]
    public async Task NoUpdate_ReportsUpToDate()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckResult = null;

        await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.Equal(UpdateStatus.UpToDate, service.Status);
    }

    [Fact]
    public async Task EngineError_MapsToFailed()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckError = new HttpRequestException("dns boom");

        await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.Equal(UpdateStatus.Failed, service.Status);
        Assert.Contains("dns boom", service.Detail);
    }

    [Fact]
    public async Task Retry_RepeatsLastFailedCheck()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckError = new HttpRequestException("dns boom");
        await service.CheckAsync(TestContext.Current.CancellationToken);
        Assert.Equal(UpdateStatus.Failed, service.Status);

        engine.CheckError = null;
        engine.CheckResult = new UpdateCheck("9.9.9", 100);
        await service.RetryAsync(TestContext.Current.CancellationToken);

        Assert.Equal(UpdateStatus.Available, service.Status);
    }

    [Fact]
    public async Task Cancel_StopsInFlightCheck()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.Gate = new TaskCompletionSource();

        var check = service.CheckAsync(TestContext.Current.CancellationToken);
        Assert.True(service.IsBusy);
        service.Cancel();
        await check;

        Assert.Equal(UpdateStatus.Idle, service.Status);
        Assert.Contains("cancel", service.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelledCheck_ReportsIdleNotFailed()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckError = new OperationCanceledException();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await service.CheckAsync(cts.Token);

        Assert.Equal(UpdateStatus.Idle, service.Status);
        Assert.Contains("cancelled", service.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelledDownload_ReportsIdleNotFailed()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckResult = new UpdateCheck("9.9.9", 2048);
        await service.CheckAsync(TestContext.Current.CancellationToken);
        Assert.Equal(UpdateStatus.Available, service.Status);

        engine.DownloadError = new OperationCanceledException();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await service.DownloadAndRestartAsync(cts.Token);

        Assert.Equal(UpdateStatus.Idle, service.Status);
        Assert.Contains("cancelled", service.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.False(engine.Restarted);
    }

    [Fact]
    public async Task BetaChannel_SelectsBetaFeed()
    {
        var (service, _) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/stable", BetaFeedUrl = "https://example.test/beta" },
            new ShellSettings { UpdateChannel = UpdateChannel.Beta });

        Assert.Equal("https://example.test/beta", service.EffectiveFeedUrl);
    }

    [Fact]
    public async Task DeltaUpdate_IsSurfaced()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" });
        engine.CheckResult = new UpdateCheck("9.9.9", 2048, IsDelta: true);

        await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.Contains("delta", service.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Downgrade_IsDetected_AndForwardedToEngine()
    {
        var (service, engine) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/feed" },
            currentVersion: "2.0.0");
        engine.CheckResult = new UpdateCheck("1.5.0", 2048);

        await service.CheckAsync(TestContext.Current.CancellationToken, allowDowngrade: true);

        Assert.True(service.IsDowngrade);
        Assert.True(engine.LastAllowDowngrade);
        Assert.Contains("older", service.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CurrentVersion_IsReported()
    {
        var (service, _) = await CreateAsync(currentVersion: "1.2.3");

        Assert.Equal("1.2.3", service.CurrentVersion);
        Assert.Null(service.AvailableVersion);
    }
}
