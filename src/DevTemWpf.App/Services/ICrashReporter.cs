namespace DevTemWpf.App.Services;

/// <summary>
/// A ready-to-persist crash record. <see cref="Content"/> is already redacted
/// (<see cref="LogPrivacy"/>) so every sink — local file or a user-supplied
/// adapter — starts from a scrubbed payload (ADR-0017/D-1).
/// </summary>
/// <param name="FilePath">Suggested per-user path for a local sink.</param>
/// <param name="Summary">One-line, redacted summary.</param>
/// <param name="Context">Where the crash was caught.</param>
/// <param name="Timestamp">Capture time.</param>
/// <param name="Content">Redacted, human-readable full report.</param>
public sealed record CrashReport(
    string FilePath,
    string Summary,
    string Context,
    DateTimeOffset Timestamp,
    string Content);

/// <summary>
/// Adapter seam for a user-chosen crash reporter (local file by default; e.g.
/// an opt-in exporter). No sink performs network I/O by default.
/// </summary>
public interface ICrashSink
{
    Task WriteAsync(CrashReport report, CancellationToken cancellationToken = default);
}

/// <summary>
/// Local-first crash capture (ADR-0017/D-1): no vendor, no telemetry. Records
/// unhandled exceptions to a redacted per-user file so a crash is diagnosable
/// without shipping data anywhere.
/// </summary>
public interface ICrashReporter
{
    /// <summary>True once the unhandled-exception handlers are attached.</summary>
    bool IsInstalled { get; }

    /// <summary>Attaches process/dispatcher/task handlers when enabled.</summary>
    void Install();

    /// <summary>Captures an exception now (also usable for caught exceptions).</summary>
    Task<CrashReport> CaptureAsync(Exception exception, string context = "", CancellationToken cancellationToken = default);
}
