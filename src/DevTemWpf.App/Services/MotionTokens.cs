using System.Windows.Media.Animation;

namespace DevTemWpf.App.Services;

/// <summary>
/// Motion token values (documented in <c>docs/design-system/tokens.md</c>).
/// Single source for durations, easings, and distances used by
/// <see cref="MotionService"/> and view-level storyboards.
/// </summary>
public static class MotionTokens
{
    public static TimeSpan Instant { get; } = TimeSpan.Zero;
    public static TimeSpan Fast { get; } = TimeSpan.FromMilliseconds(120);
    public static TimeSpan Standard { get; } = TimeSpan.FromMilliseconds(220);
    public static TimeSpan Deliberate { get; } = TimeSpan.FromMilliseconds(380);

    /// <summary>Maximum duration of any transition: motion never delays actions.</summary>
    public static TimeSpan MaxDuration { get; } = TimeSpan.FromMilliseconds(380);

    public const double SlideDistance = 12.0;
    public const double RippleStartDiameter = 8.0;
    public const double RippleEndDiameter = 220.0;

    public static EasingFunctionBase EaseOutCubic() => new CubicEase { EasingMode = EasingMode.EaseOut };

    public static EasingFunctionBase EaseInOutCubic() => new CubicEase { EasingMode = EasingMode.EaseInOut };
}
