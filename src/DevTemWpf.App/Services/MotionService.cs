using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DevTemWpf.App.Services;

/// <summary>
/// Lightweight storyboard motion (no shader/compositor dependencies).
/// All failures degrade to the final state — motion never crashes a view.
/// </summary>
public sealed class MotionService : IMotionService
{
    private readonly ISettingsService _settings;
    private readonly Func<bool> _osAnimationsEnabled;
    private readonly Func<int> _renderTier;

    public MotionService(
        ISettingsService settings,
        Func<bool>? osAnimationsEnabled = null,
        Func<int>? renderTier = null)
    {
        _settings = settings;
        _osAnimationsEnabled = osAnimationsEnabled ?? OsMotionInfo.IsAnimationEnabled;
        _renderTier = renderTier ?? (() => RenderCapability.Tier >> 16);
    }

    public EffectiveMotion CurrentPolicy
        => MotionPolicy.Resolve(_settings.Current.Motion, _osAnimationsEnabled(), RenderTier);

    public bool OsReducedMotion => !_osAnimationsEnabled();

    public int RenderTier
    {
        get
        {
            try
            {
                return _renderTier();
            }
            catch
            {
                return 2;
            }
        }
    }

    public bool AllowsEffect(MotionEffect effect)
    {
        var settings = _settings.Current;
        if (!settings.EffectsEnabled || !MotionPolicy.AllowsDecorative(CurrentPolicy))
        {
            return false;
        }

        return effect switch
        {
            MotionEffect.Ripple => settings.RippleEffectEnabled,
            MotionEffect.Gradient => settings.GradientEffectEnabled,
            MotionEffect.Warp => settings.WarpEffectEnabled,
            _ => false,
        };
    }

    public Task PlayEntranceAsync(FrameworkElement element, TimeSpan? durationOverride = null, CancellationToken cancellationToken = default)
    {
        try
        {
            element.Dispatcher.VerifyAccess();
            var policy = CurrentPolicy;
            if (policy == EffectiveMotion.Off || cancellationToken.IsCancellationRequested)
            {
                SnapEntrance(element);
                return Task.CompletedTask;
            }

            var duration = durationOverride
                ?? (policy == EffectiveMotion.Reduced ? MotionTokens.Fast : MotionTokens.Standard);
            var storyboard = new Storyboard { FillBehavior = FillBehavior.Stop };

            var fade = new DoubleAnimation(0, 1, duration);
            Storyboard.SetTarget(fade, element);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fade);

            TranslateTransform? slide = null;
            if (MotionPolicy.AllowsMovement(policy))
            {
                slide = element.RenderTransform as TranslateTransform;
                if (slide is null)
                {
                    slide = new TranslateTransform();
                    element.RenderTransform = slide;
                }

                var move = new DoubleAnimation(MotionTokens.SlideDistance, 0, duration)
                {
                    EasingFunction = MotionTokens.EaseOutCubic(),
                };
                Storyboard.SetTarget(move, slide);
                Storyboard.SetTargetProperty(move, new PropertyPath(TranslateTransform.YProperty));
                storyboard.Children.Add(move);
            }

            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            storyboard.Completed += (_, _) =>
            {
                SnapEntrance(element);
                completion.TrySetResult();
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() =>
                    element.Dispatcher.BeginInvoke(() =>
                    {
                        storyboard.Stop(element);
                        SnapEntrance(element);
                        completion.TrySetResult();
                    }));
            }

            storyboard.Begin(element, true);
            return completion.Task;
        }
        catch
        {
            TrySnap(element);
            return Task.CompletedTask;
        }
    }

    public void PlayRipple(Panel host, Point origin)
    {
        try
        {
            host.Dispatcher.VerifyAccess();
            if (!AllowsEffect(MotionEffect.Ripple))
            {
                return;
            }

            var accent = Wpf.Ui.Appearance.ApplicationAccentColorManager.PrimaryAccent;
            var ellipse = new Ellipse
            {
                Width = MotionTokens.RippleStartDiameter,
                Height = MotionTokens.RippleStartDiameter,
                Stroke = new SolidColorBrush(accent),
                StrokeThickness = 2,
                Opacity = 0.8,
                IsHitTestVisible = false,
            };
            Canvas.SetLeft(ellipse, origin.X - MotionTokens.RippleStartDiameter / 2);
            Canvas.SetTop(ellipse, origin.Y - MotionTokens.RippleStartDiameter / 2);
            host.Children.Add(ellipse);

            var storyboard = new Storyboard { FillBehavior = FillBehavior.Stop };
            var grow = new DoubleAnimation(MotionTokens.RippleStartDiameter, MotionTokens.RippleEndDiameter, MotionTokens.Deliberate)
            {
                EasingFunction = MotionTokens.EaseOutCubic(),
            };
            Storyboard.SetTarget(grow, ellipse);
            Storyboard.SetTargetProperty(grow, new PropertyPath(FrameworkElement.WidthProperty));
            storyboard.Children.Add(grow);

            var growH = new DoubleAnimation(MotionTokens.RippleStartDiameter, MotionTokens.RippleEndDiameter, MotionTokens.Deliberate)
            {
                EasingFunction = MotionTokens.EaseOutCubic(),
            };
            Storyboard.SetTarget(growH, ellipse);
            Storyboard.SetTargetProperty(growH, new PropertyPath(FrameworkElement.HeightProperty));
            storyboard.Children.Add(growH);

            var fade = new DoubleAnimation(0.8, 0, MotionTokens.Deliberate);
            Storyboard.SetTarget(fade, ellipse);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fade);

            // Removal is idempotent and dual-pathed: storyboard completion
            // covers the live tree, while the timed fallback covers early
            // unload and environments where clocks never tick (e.g. tests).
            storyboard.Completed += (_, _) => RemoveRipple(host, ellipse);
            // Parameterless Begin: targets are set explicitly, and controllable
            // Begin can stall on elements outside a live visual tree.
            storyboard.Begin();
            var dispatcher = host.Dispatcher;
            _ = Task.Delay(MotionTokens.Deliberate + TimeSpan.FromMilliseconds(500))
                .ContinueWith(
                    _ => dispatcher.InvokeAsync(() => RemoveRipple(host, ellipse)),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
        }
        catch
        {
            // Decorative only: never fail a click because of a ripple.
        }
    }

    private static void RemoveRipple(Panel host, UIElement element)
    {
        try
        {
            host.Children.Remove(element);
        }
        catch
        {
            // Best effort: the host may be gone already.
        }
    }

    private static void SnapEntrance(FrameworkElement element)    {
        element.Opacity = 1;
        if (element.RenderTransform is TranslateTransform slide)
        {
            slide.Y = 0;
        }
    }

    private static void TrySnap(FrameworkElement element)
    {
        try
        {
            if (element.Dispatcher.CheckAccess())
            {
                SnapEntrance(element);
            }
        }
        catch
        {
            // Last resort: leave the element as-is.
        }
    }
}
