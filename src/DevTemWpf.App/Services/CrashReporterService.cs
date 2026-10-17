using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevTemWpf.App.Services;

/// <summary>
/// Default <see cref="ICrashReporter"/>: builds a redacted report and hands it
/// to an <see cref="ICrashSink"/> (local file by default). Never throws to the
/// caller and never performs network I/O (ADR-0017/D-1).
/// </summary>
public sealed class CrashReporterService : ICrashReporter
{
    private readonly CrashOptions _options;
    private readonly ICrashSink _sink;
    private readonly ILogger<CrashReporterService>? _logger;
    private readonly string _crashDir;

    public CrashReporterService(
        IOptions<CrashOptions> options,
        ICrashSink? sink = null,
        ILogger<CrashReporterService>? logger = null,
        string? crashDir = null)
    {
        _options = options.Value;
        _crashDir = crashDir ?? AppPaths.CrashDir;
        _sink = sink ?? new LocalFileCrashSink();
        _logger = logger;
    }

    public bool IsInstalled { get; private set; }

    public void Install()
    {
        if (IsInstalled || !_options.Enabled)
        {
            return;
        }

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        if (Application.Current is { } app)
        {
            app.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        IsInstalled = true;
    }

    public async Task<CrashReport> CaptureAsync(Exception exception, string context = "", CancellationToken cancellationToken = default)
    {
        var report = Build(exception, context);
        try
        {
            await _sink.WriteAsync(report, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception sinkError)
        {
            _logger?.LogWarning(sinkError, "Crash sink failed; crash was not persisted.");
        }

        return report;
    }

    private CrashReport Build(Exception exception, string context)
    {
        var timestamp = DateTimeOffset.Now;
        var summary = LogPrivacy.Redact($"{exception.GetType().Name}: {exception.Message}");

        var builder = new StringBuilder();
        builder.AppendLine($"# Crash report {timestamp:o}");
        builder.AppendLine($"Version: {typeof(CrashReporterService).Assembly.GetName().Version}");
        builder.AppendLine($"OS: {Environment.OSVersion.VersionString}");
        builder.AppendLine($"Runtime: {Environment.Version}");
        builder.AppendLine($"Context: {context}");
        builder.AppendLine($"Type: {exception.GetType().FullName}");
        builder.AppendLine("---");
        builder.AppendLine(exception.ToString());

        var fileName = $"crash-{timestamp:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.txt";
        var path = Path.Combine(_crashDir, fileName);
        return new CrashReport(path, summary, context, timestamp, LogPrivacy.Redact(builder.ToString()));
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            CaptureBlocking(exception, "AppDomain.UnhandledException");
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        => CaptureBlocking(e.Exception, "DispatcherUnhandledException");

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        CaptureBlocking(e.Exception, "TaskScheduler.UnobservedTaskException");
        e.SetObserved();
    }

    private void CaptureBlocking(Exception exception, string context)
    {
        try
        {
            // The process may be terminating; a brief synchronous write is the
            // only reliable way to persist the report before exit.
            CaptureAsync(exception, context).GetAwaiter().GetResult();
        }
        catch
        {
            // Never let crash handling introduce a second failure.
        }
    }
}

/// <summary>Writes crash reports to a per-user directory (no upload).</summary>
public sealed class LocalFileCrashSink : ICrashSink
{
    public async Task WriteAsync(CrashReport report, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(report.FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(report.FilePath, report.Content, cancellationToken).ConfigureAwait(false);
    }
}
