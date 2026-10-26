namespace DevTemWpf.App.Services;

/// <summary>
/// Crash-capture configuration (ADR-0017/D-1). The module flag is the opt-in;
/// this switch lets a host turn capture off at runtime. Capture is always
/// local — there is no network telemetry regardless of this value.
/// </summary>
public sealed class CrashOptions
{
    public const string SectionName = "Crash";

    public bool Enabled { get; set; } = true;
}
