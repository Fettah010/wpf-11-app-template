using DevTemWpf.App.Services;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Unit;

/// <summary>
/// AC04/AC08: local-first crash capture never throws, writes a redacted file,
/// supports an adapter sink, and is inert when disabled.
/// </summary>
public sealed class CrashReporterServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "devtem-crash-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
    }

    private sealed class FakeSink : ICrashSink
    {
        public CrashReport? Last { get; private set; }

        public Exception? Throw { get; set; }

        public Task WriteAsync(CrashReport report, CancellationToken cancellationToken = default)
        {
            if (Throw is not null)
            {
                return Task.FromException(Throw);
            }

            Last = report;
            return Task.CompletedTask;
        }
    }

    private CrashReporterService Create(CrashOptions? options = null, ICrashSink? sink = null)
        => new(Options.Create(options ?? new CrashOptions()), sink, logger: null, crashDir: _dir);

    [Fact]
    public async Task CaptureAsync_WritesReportFile()
    {
        var service = Create();

        var report = await service.CaptureAsync(new InvalidOperationException("boom"), "unit", TestContext.Current.CancellationToken);

        Assert.True(File.Exists(report.FilePath));
        Assert.Contains("InvalidOperationException", report.Content);
        Assert.Contains("unit", report.Content);
        Assert.StartsWith(report.FilePath, Path.Combine(_dir, Path.GetFileName(report.FilePath)));
    }

    [Fact]
    public async Task CaptureAsync_RedactsSecretShapedValues()
    {
        var service = Create();

        var report = await service.CaptureAsync(
            new InvalidOperationException("login failed password=hunter2 apiKey=deadbeef"),
            "auth",
            TestContext.Current.CancellationToken);

        Assert.DoesNotContain("hunter2", report.Content);
        Assert.DoesNotContain("hunter2", report.Summary);
        Assert.Contains(LogPrivacy.Mask, report.Content);
    }

    [Fact]
    public async Task CaptureAsync_UsesProvidedSink()
    {
        var sink = new FakeSink();
        var service = Create(sink: sink);

        await service.CaptureAsync(new Exception("boom"), "ctx", TestContext.Current.CancellationToken);

        Assert.NotNull(sink.Last);
        Assert.Equal("ctx", sink.Last!.Context);
    }

    [Fact]
    public async Task CaptureAsync_SinkFailure_DoesNotThrow()
    {
        var sink = new FakeSink { Throw = new IOException("disk full") };
        var service = Create(sink: sink);

        var report = await service.CaptureAsync(new Exception("boom"), "ctx", TestContext.Current.CancellationToken);

        Assert.NotNull(report);
    }

    [Fact]
    public void Install_WhenDisabled_DoesNotInstall()
    {
        var service = Create(new CrashOptions { Enabled = false });

        service.Install();

        Assert.False(service.IsInstalled);
    }

    [Fact]
    public void Install_WhenEnabled_IsInstalledAndIdempotent()
    {
        var service = Create(sink: new FakeSink());

        service.Install();
        service.Install();

        Assert.True(service.IsInstalled);
    }
}
