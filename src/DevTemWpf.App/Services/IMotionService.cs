using System.Windows;

namespace DevTemWpf.App.Services;

/// <summary>Storyboard-based motion honoring the resolved policy (ADR-0009, ADR-0016).</summary>
public interface IMotionService
{
    EffectiveMotion CurrentPolicy { get; }

    bool OsReducedMotion { get; }

    int RenderTier { get; }

    /// <summary>
    /// Whether a specific decorative effect is currently allowed: master
    /// <c>EffectsEnabled</c> switch on, policy is Rich, and the effect's own
    /// switch is on.
    /// </summary>
    bool AllowsEffect(MotionEffect effect);

    /// <summary>
    /// Page/content entrance. Off policy (or any failure) snaps the final
    /// state and returns a completed task — callers never wait on animation.
    /// <paramref name="durationOverride"/> demonstrates a preset speed;
    /// policy gating (instant/slide rules) still applies.
    /// </summary>
    Task PlayEntranceAsync(FrameworkElement element, TimeSpan? durationOverride = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decorative click ripple. No-op unless <see cref="AllowsEffect"/> allows
    /// <see cref="MotionEffect.Ripple"/>.
    /// </summary>
    void PlayRipple(System.Windows.Controls.Panel host, Point origin);
}
