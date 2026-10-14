namespace DevTemWpf.App.Services;

/// <summary>Motion richness preference (ADR-0009).</summary>
public enum MotionMode
{
    /// <summary>OS animation setting + render tier decide (default).</summary>
    Auto = 0,
    /// <summary>Everything the system offers.</summary>
    Full = 1,
    /// <summary>Fast fades only; no slide, ripple, or looping surfaces.</summary>
    Reduced = 2,
    /// <summary>Instant state changes; no storyboards.</summary>
    Off = 3,
}

/// <summary>Resolved, environment-aware motion level.</summary>
public enum EffectiveMotion
{
    Off = 0,
    Reduced = 1,
    Full = 2,
}
