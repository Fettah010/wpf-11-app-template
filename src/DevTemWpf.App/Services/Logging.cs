using System.IO;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DevTemWpf.App.Services;

/// <summary>
/// Serilog file logging (ADR-0011). App-only composition: tests never call
/// this (no log files from test runs). Rolling daily files, 14-day retention.
/// </summary>
public static class Logging
{
    public static HostApplicationBuilder AddAppLogging(this HostApplicationBuilder builder)
    {
        Directory.CreateDirectory(AppPaths.LogsDir);
        builder.Services.AddSerilog((_, config) => config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(AppPaths.LogsDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:o} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));
        return builder;
    }
}
