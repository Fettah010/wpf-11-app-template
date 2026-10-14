using DevTemWpf.App.Services;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Unit;

public sealed class UpdateServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    private sealed class FakeEngine : IUpdateEngine
    {
        public UpdateCheck? CheckResult { get; set; }
        public Exception? CheckError { get; set; }
        public bool Restarted { get; private set; }

        public Task<UpdateCheck?> CheckAsync(string feedUrl, CancellationToken cancellationToken)
            => CheckError is not null ? Task.FromException<UpdateCheck?>(CheckError) : Task.FromResult(CheckResult);

        public Task DownloadAsync(string feedUrl, UpdateCheck update, IProgress<double> progress, CancellationToken cancellationToken)
        {
            progress.Report(0.5);
            progress.Report(1.0);
            return Task.CompletedTask;
        }

        public void ApplyAndRestart(string feedUrl, UpdateCheck update) => Restarted = true;
    }

    private async Task<(UpdateService Service, FakeEngine Engine)> CreateAsync(
        UpdateOptions? options = null, ShellSettings? seed = null)
    {
        var settings = new SettingsService(_path);
        await settings.UpdateAsync(_ => seed ?? new ShellSettings(), TestContext.Current.CancellationToken);
        var engine = new FakeEngine();
        return (new UpdateService(settings, Options.Create(options ?? new UpdateOptions()), () => engine), engine);
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

        await service.DownloadAndRestartAsync(TestContext.Current.CancellationToken);
        Assert.Equal(UpdateStatus.ReadyToRestart, service.Status);
        Assert.True(engine.Restarted);

        // Progress<T> posts asynchronously: wait for the final report.
        // Generous window: the MTP runner executes tests in parallel.
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (service.Progress < 1.0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25, TestContext.Current.CancellationToken);
        }

        Assert.Equal(1.0, service.Progress);
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
    public async Task BetaChannel_SelectsBetaFeed()
    {
        var (service, _) = await CreateAsync(
            new UpdateOptions { StableFeedUrl = "https://example.test/stable", BetaFeedUrl = "https://example.test/beta" },
            new ShellSettings { UpdateChannel = UpdateChannel.Beta });

        Assert.Equal("https://example.test/beta", service.EffectiveFeedUrl);
    }
}
