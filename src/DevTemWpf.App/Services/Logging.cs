using System.IO;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DevTemWpf.App.Services;

/// <summary>
/// Serilog file logging (ADR-0011; bounded + private in 1.6.0, ADR-0018).
/// App-only composition: tests never call this (no log files from test runs).
/// </summary>
public static class Logging
{
    /// <summary>Bounded retention: no more than 14 daily/rolled files.</summary>
    public const int RetainedFileCountLimit = 14;

    /// <summary>Bounded size: each file rolls at 5 MB.</summary>
    public const long FileSizeLimitBytes = 5 * 1024 * 1024;

    /// <summary>Shared line layout (redacted by <see cref="RedactingTextFormatter"/>).</summary>
    public const string OutputTemplate =
        "{Timestamp:o} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

    public static HostApplicationBuilder AddAppLogging(this HostApplicationBuilder builder)
    {
        Directory.CreateDirectory(AppPaths.LogsDir);
        builder.Services.AddSerilog((_, config) => config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                new RedactingTextFormatter(OutputTemplate),
                Path.Combine(AppPaths.LogsDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: FileSizeLimitBytes,
                retainedFileCountLimit: RetainedFileCountLimit));
        return builder;
    }
}
