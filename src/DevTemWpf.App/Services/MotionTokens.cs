using System.Windows.Media.Animation;

namespace DevTemWpf.App.Services;

/// <summary>
/// Motion token values (documented in <c>docs/design-system/tokens.md</c> and
/// <c>docs/design-system/motion.md</c>). Single source for durations, easings,
/// distances, opacity, and stagger used by <see cref="MotionService"/> and
/// view-level storyboards — views must not hard-code motion literals.
/// </summary>
public static class MotionTokens
{
    /// <summary>State jumps; no storyboard.</summary>
    public static TimeSpan Instant { get; } = TimeSpan.Zero;

    /// <summary>Opacity-only micro-feedback (reduced-motion duration).</summary>
    public static TimeSpan Fast { get; } = TimeSpan.FromMilliseconds(120);

    /// <summary>Standard transitions and page entrances.</summary>
    public static TimeSpan Standard { get; } = TimeSpan.FromMilliseconds(220);

    /// <summary>Expressive transitions (ripple expand, deliberate feedback).</summary>
    public static TimeSpan Deliberate { get; } = TimeSpan.FromMilliseconds(380);

    /// <summary>Per-item cascade step for staggered content.</summary>
    public static TimeSpan Stagger { get; } = TimeSpan.FromMilliseconds(40);

    /// <summary>Maximum duration of any transition: motion never delays actions.</summary>
    public static TimeSpan MaxDuration { get; } = TimeSpan.FromMilliseconds(380);

    /// <summary>Entrance rise distance (device-independent pixels).</summary>
    public const double SlideDistance = 12.0;

    /// <summary>Exit fall distance (least-visible direction, half the entrance rise).</summary>
    public const double ExitDistance = 6.0;

    public const double RippleStartDiameter = 8.0;
    public const double RippleEndDiameter = 220.0;

    /// <summary>Opacity an entering element animates from.</summary>
    public const double EnterFromOpacity = 0.0;

    /// <summary>Resting opacity of a fully visible element.</summary>
    public const double RestOpacity = 1.0;

    /// <summary>Opacity an exiting element animates to.</summary>
    public const double ExitToOpacity = 0.0;

    /// <summary>Peak opacity of a transient feedback flash.</summary>
    public const double FeedbackPeakOpacity = 0.85;

    /// <summary>Scale an element pulses to during feedback (1.0 = rest).</summary>
    public const double FeedbackPeakScale = 1.03;

    /// <summary>Opacity of a selected indicator.</summary>
    public const double SelectionOnOpacity = 1.0;

    /// <summary>Opacity of a deselected indicator.</summary>
    public const double SelectionOffOpacity = 0.0;

    public static EasingFunctionBase EaseOutCubic() => new CubicEase { EasingMode = EasingMode.EaseOut };

    public static EasingFunctionBase EaseInOutCubic() => new CubicEase { EasingMode = EasingMode.EaseInOut };
}
