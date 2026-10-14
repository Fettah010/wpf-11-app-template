namespace DevTemWpf.App.Services;

/// <summary>Pure policy mapping: preference + environment → effective motion.</summary>
public static class MotionPolicy
{
    public static EffectiveMotion Resolve(MotionMode mode, bool osAnimationsEnabled, int renderTier)
        => mode switch
        {
            MotionMode.Off => EffectiveMotion.Off,
            MotionMode.Reduced => EffectiveMotion.Reduced,
            MotionMode.Full => EffectiveMotion.Full,
            _ when !osAnimationsEnabled => EffectiveMotion.Reduced,
            _ when renderTier <= 0 => EffectiveMotion.Reduced,
            _ => EffectiveMotion.Full,
        };
}
