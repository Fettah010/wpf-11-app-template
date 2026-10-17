namespace DevTemWpf.App.Services;

/// <summary>
/// A premium/decorative effect that can be individually enabled (1.5.0,
/// ADR-0016). All effects are additionally gated by the master
/// <c>ShellSettings.EffectsEnabled</c> switch and the Rich motion level.
/// </summary>
public enum MotionEffect
{
    /// <summary>Click/origin ripple on a host panel.</summary>
    Ripple = 0,
    /// <summary>Looping animated gradient surface.</summary>
    Gradient = 1,
    /// <summary>Backdrop distortion prototype (simulated, backend-only).</summary>
    Warp = 2,
}
