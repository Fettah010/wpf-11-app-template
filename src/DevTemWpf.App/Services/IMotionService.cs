using System.Windows;

namespace DevTemWpf.App.Services;

/// <summary>
/// Storyboard-based motion honoring the resolved policy (ADR-0009, ADR-0016).
/// Every primitive is cancellable and snaps to its final state on
/// cancellation or failure, so callers can await without blocking.
/// </summary>
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
    /// Content entrance (fade, plus rise when movement is allowed). Off policy
    /// (or any failure) snaps the final state and returns a completed task.
    /// <paramref name="durationOverride"/> demonstrates a preset speed; policy
    /// gating (instant/slide rules) still applies.
    /// </summary>
    Task PlayEntranceAsync(FrameworkElement element, TimeSpan? durationOverride = null, CancellationToken cancellationToken = default);

    /// <summary>Content exit (fade, plus fall when movement is allowed).</summary>
    Task PlayExitAsync(FrameworkElement element, CancellationToken cancellationToken = default);

    /// <summary>Animate an indicator between selected and deselected opacity.</summary>
    Task PlaySelectionAsync(FrameworkElement indicator, bool selected, CancellationToken cancellationToken = default);

    /// <summary>Reveal detail content (fade, plus a short rise when movement is allowed).</summary>
    Task PlayExpandAsync(FrameworkElement element, CancellationToken cancellationToken = default);

    /// <summary>Hide detail content (fade, plus a short fall when movement is allowed).</summary>
    Task PlayCollapseAsync(FrameworkElement element, CancellationToken cancellationToken = default);

    /// <summary>Transient acknowledgement pulse (opacity flash; scale when movement is allowed).</summary>
    Task PlayFeedbackAsync(FrameworkElement element, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reduced-motion-aware page transition: a directional fade + slide
    /// (<paramref name="forward"/> chooses the direction); Reduced falls back
    /// to a fast fade and Off snaps.
    /// </summary>
    Task PlayPageTransitionAsync(FrameworkElement element, bool forward, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decorative click ripple. No-op unless <see cref="AllowsEffect"/> allows
    /// <see cref="MotionEffect.Ripple"/>.
    /// </summary>
    void PlayRipple(System.Windows.Controls.Panel host, Point origin);
}
