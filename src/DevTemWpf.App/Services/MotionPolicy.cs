namespace DevTemWpf.App.Services;

/// <summary>
/// Pure policy mapping: preference + environment → effective motion
/// (ADR-0009, ADR-0016). No I/O, no UI — trivially unit-testable.
/// </summary>
public static class MotionPolicy
{
    /// <summary>
    /// Resolves the effective level. Explicit modes pass through; <c>Auto</c>
    /// honours the OS animation setting and render tier and otherwise settles
    /// on <see cref="EffectiveMotion.Balanced"/> (the 1.5.0 default) — the
    /// maximum <see cref="EffectiveMotion.Full"/> tier stays an explicit choice.
    /// </summary>
    public static EffectiveMotion Resolve(MotionMode mode, bool osAnimationsEnabled, int renderTier)
        => mode switch
        {
            MotionMode.Off => EffectiveMotion.Off,
            MotionMode.Reduced => EffectiveMotion.Reduced,
            MotionMode.Balanced => EffectiveMotion.Balanced,
            MotionMode.Full => EffectiveMotion.Full,
            _ when !osAnimationsEnabled => EffectiveMotion.Reduced,
            _ when renderTier <= 0 => EffectiveMotion.Reduced,
            _ => EffectiveMotion.Balanced,
        };

    /// <summary>Movement (slide/rise/scale) is allowed from Balanced up.</summary>
    public static bool AllowsMovement(EffectiveMotion policy) => policy >= EffectiveMotion.Balanced;

    /// <summary>Decorative/looping effects (ripple, gradient, warp) require Rich.</summary>
    public static bool AllowsDecorative(EffectiveMotion policy) => policy == EffectiveMotion.Full;
}
