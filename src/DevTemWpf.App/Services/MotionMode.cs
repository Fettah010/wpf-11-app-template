namespace DevTemWpf.App.Services;

/// <summary>
/// Motion richness preference (ADR-0009, ADR-0016). Persisted by integer in
/// <c>ShellSettings.Motion</c>; existing members must never be renumbered —
/// new members are appended (1.5.0 added <see cref="Balanced"/>).
/// </summary>
public enum MotionMode
{
    /// <summary>OS animation setting + render tier decide (default).</summary>
    Auto = 0,
    /// <summary>Everything the system offers — including decorative/looping
    /// effects. Shown as "Rich" in the UI.</summary>
    Full = 1,
    /// <summary>Fast fades only; no slide, ripple, or looping surfaces.</summary>
    Reduced = 2,
    /// <summary>Instant state changes; no storyboards.</summary>
    Off = 3,
    /// <summary>Standard transitions and movement, but no decorative or
    /// looping effects (1.5.0).</summary>
    Balanced = 4,
}

/// <summary>
/// Resolved, environment-aware motion level. Derived at runtime (never
/// persisted), ordered by richness so <c>&gt;=</c>/<c>&lt;=</c> comparisons read
/// naturally (Off &lt; Reduced &lt; Balanced &lt; Full/Rich).
/// </summary>
public enum EffectiveMotion
{
    Off = 0,
    Reduced = 1,
    Balanced = 2,
    Full = 3,
}
