using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DevTemWpf.App.Services;

/// <summary>
/// Lightweight storyboard motion (no shader/compositor dependencies).
/// All primitives are policy-gated, cancellable, and snap to their final
/// state on cancellation or any failure — motion never crashes a view and
/// never leaves a surface stuck mid-transition (ADR-0009, ADR-0016).
/// Primitives assume they own the element's <see cref="UIElement.RenderTransform"/>
/// (creating or appending a transform as needed).
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
        var policy = CurrentPolicy;
        var move = MotionPolicy.AllowsMovement(policy);
        var duration = durationOverride ?? FadeDuration(policy);
        return RunAsync(element, duration, (sb, el, d) =>
        {
            AddFade(sb, el, el.Opacity, MotionTokens.RestOpacity, d);
            if (move)
            {
                AddTranslate(sb, EnsureTransform<TranslateTransform>(el), TranslateTransform.YProperty, MotionTokens.SlideDistance, 0, d);
            }
        }, el =>
        {
            el.Opacity = MotionTokens.RestOpacity;
            ResetTranslate(el);
        }, cancellationToken);
    }

    public Task PlayExitAsync(FrameworkElement element, CancellationToken cancellationToken = default)
    {
        var policy = CurrentPolicy;
        var move = MotionPolicy.AllowsMovement(policy);
        return RunAsync(element, FadeDuration(policy), (sb, el, d) =>
        {
            AddFade(sb, el, el.Opacity, MotionTokens.ExitToOpacity, d);
            if (move)
            {
                AddTranslate(sb, EnsureTransform<TranslateTransform>(el), TranslateTransform.YProperty, 0, MotionTokens.ExitDistance, d);
            }
        }, el =>
        {
            el.Opacity = MotionTokens.ExitToOpacity;
            if (move)
            {
                SetTranslateY(el, MotionTokens.ExitDistance);
            }
            else
            {
                ResetTranslate(el);
            }
        }, cancellationToken);
    }

    public Task PlaySelectionAsync(FrameworkElement indicator, bool selected, CancellationToken cancellationToken = default)
    {
        var target = selected ? MotionTokens.SelectionOnOpacity : MotionTokens.SelectionOffOpacity;
        var policy = CurrentPolicy;
        return RunAsync(indicator, FadeDuration(policy), (sb, el, d) =>
            AddFade(sb, el, el.Opacity, target, d, MotionTokens.EaseInOutCubic()),
            el => el.Opacity = target, cancellationToken);
    }

    public Task PlayExpandAsync(FrameworkElement element, CancellationToken cancellationToken = default)
    {
        var policy = CurrentPolicy;
        var move = MotionPolicy.AllowsMovement(policy);
        return RunAsync(element, FadeDuration(policy), (sb, el, d) =>
        {
            AddFade(sb, el, el.Opacity, MotionTokens.RestOpacity, d);
            if (move)
            {
                AddTranslate(sb, EnsureTransform<TranslateTransform>(el), TranslateTransform.YProperty, -MotionTokens.ExitDistance, 0, d);
            }
        }, el =>
        {
            el.Opacity = MotionTokens.RestOpacity;
            ResetTranslate(el);
        }, cancellationToken);
    }

    public Task PlayCollapseAsync(FrameworkElement element, CancellationToken cancellationToken = default)
    {
        var policy = CurrentPolicy;
        var move = MotionPolicy.AllowsMovement(policy);
        return RunAsync(element, FadeDuration(policy), (sb, el, d) =>
        {
            AddFade(sb, el, el.Opacity, MotionTokens.ExitToOpacity, d);
            if (move)
            {
                AddTranslate(sb, EnsureTransform<TranslateTransform>(el), TranslateTransform.YProperty, 0, -MotionTokens.ExitDistance, d);
            }
        }, el =>
        {
            el.Opacity = MotionTokens.ExitToOpacity;
            if (move)
            {
                SetTranslateY(el, -MotionTokens.ExitDistance);
            }
            else
            {
                ResetTranslate(el);
            }
        }, cancellationToken);
    }

    public Task PlayFeedbackAsync(FrameworkElement element, CancellationToken cancellationToken = default)
    {
        var policy = CurrentPolicy;
        var move = MotionPolicy.AllowsMovement(policy);
        var duration = policy == EffectiveMotion.Reduced ? MotionTokens.Fast : MotionTokens.Deliberate;
        return RunAsync(element, duration, (sb, el, d) =>
        {
            AddPulse(sb, el, UIElement.OpacityProperty, el.Opacity, MotionTokens.FeedbackPeakOpacity, d);
            if (move)
            {
                var scale = EnsureTransform<ScaleTransform>(el);
                AddPulse(sb, scale, ScaleTransform.ScaleXProperty, 1.0, MotionTokens.FeedbackPeakScale, d);
                AddPulse(sb, scale, ScaleTransform.ScaleYProperty, 1.0, MotionTokens.FeedbackPeakScale, d);
            }
        }, el =>
        {
            el.Opacity = MotionTokens.RestOpacity;
            ResetScale(el);
        }, cancellationToken);
    }

    public Task PlayPageTransitionAsync(FrameworkElement element, bool forward, CancellationToken cancellationToken = default)
    {
        var policy = CurrentPolicy;
        var move = MotionPolicy.AllowsMovement(policy);
        var offset = forward ? MotionTokens.SlideDistance : -MotionTokens.SlideDistance;
        return RunAsync(element, FadeDuration(policy), (sb, el, d) =>
        {
            AddFade(sb, el, el.Opacity, MotionTokens.RestOpacity, d);
            if (move)
            {
                AddTranslate(sb, EnsureTransform<TranslateTransform>(el), TranslateTransform.XProperty, offset, 0, d);
            }
        }, el =>
        {
            el.Opacity = MotionTokens.RestOpacity;
            ResetTranslate(el);
        }, cancellationToken);
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

    private static TimeSpan FadeDuration(EffectiveMotion policy)
        => policy == EffectiveMotion.Reduced ? MotionTokens.Fast : MotionTokens.Standard;

    private Task RunAsync(
        FrameworkElement element,
        TimeSpan duration,
        Action<Storyboard, FrameworkElement, TimeSpan> build,
        Action<FrameworkElement> snap,
        CancellationToken cancellationToken)
    {
        try
        {
            element.Dispatcher.VerifyAccess();
            if (CurrentPolicy == EffectiveMotion.Off || cancellationToken.IsCancellationRequested || duration <= TimeSpan.Zero)
            {
                snap(element);
                return Task.CompletedTask;
            }

            if (duration > MotionTokens.MaxDuration)
            {
                duration = MotionTokens.MaxDuration;
            }

            var storyboard = new Storyboard { FillBehavior = FillBehavior.Stop };
            build(storyboard, element, duration);

            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            storyboard.Completed += (_, _) =>
            {
                snap(element);
                completion.TrySetResult();
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() =>
                    element.Dispatcher.BeginInvoke(() =>
                    {
                        storyboard.Stop(element);
                        snap(element);
                        completion.TrySetResult();
                    }));
            }

            storyboard.Begin(element, true);
            return completion.Task;
        }
        catch
        {
            TryInvoke(element, snap);
            return Task.CompletedTask;
        }
    }

    private static void AddFade(
        Storyboard sb,
        FrameworkElement element,
        double from,
        double to,
        TimeSpan duration,
        EasingFunctionBase? easing = null)
    {
        var fade = new DoubleAnimation(from, to, duration) { EasingFunction = easing };
        Storyboard.SetTarget(fade, element);
        Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
        sb.Children.Add(fade);
    }

    private static void AddTranslate(
        Storyboard sb,
        TranslateTransform transform,
        DependencyProperty property,
        double from,
        double to,
        TimeSpan duration)
    {
        var move = new DoubleAnimation(from, to, duration) { EasingFunction = MotionTokens.EaseOutCubic() };
        Storyboard.SetTarget(move, transform);
        Storyboard.SetTargetProperty(move, new PropertyPath(property));
        sb.Children.Add(move);
    }

    private static void AddPulse(
        Storyboard sb,
        DependencyObject target,
        DependencyProperty property,
        double rest,
        double peak,
        TimeSpan duration)
    {
        var pulse = new DoubleAnimationUsingKeyFrames();
        var mid = TimeSpan.FromTicks(duration.Ticks / 2);
        pulse.KeyFrames.Add(new EasingDoubleKeyFrame(rest, KeyTime.FromTimeSpan(TimeSpan.Zero), MotionTokens.EaseInOutCubic()));
        pulse.KeyFrames.Add(new EasingDoubleKeyFrame(peak, KeyTime.FromTimeSpan(mid), MotionTokens.EaseInOutCubic()));
        pulse.KeyFrames.Add(new EasingDoubleKeyFrame(rest, KeyTime.FromTimeSpan(duration), MotionTokens.EaseInOutCubic()));
        Storyboard.SetTarget(pulse, target);
        Storyboard.SetTargetProperty(pulse, new PropertyPath(property));
        sb.Children.Add(pulse);
    }

    private static T EnsureTransform<T>(FrameworkElement element)
        where T : Transform, new()
    {
        if (element.RenderTransform is T existing)
        {
            return existing;
        }

        var created = new T();
        if (element.RenderTransform is null || element.RenderTransform == Transform.Identity)
        {
            element.RenderTransform = created;
        }
        else
        {
            var group = new TransformGroup();
            group.Children.Add(element.RenderTransform);
            group.Children.Add(created);
            element.RenderTransform = group;
        }

        return created;
    }

    private static T? FindTransform<T>(FrameworkElement element)
        where T : Transform
        => FindTransform<T>(element.RenderTransform);

    private static T? FindTransform<T>(Transform? transform)
        where T : Transform
    {
        switch (transform)
        {
            case null:
                return null;
            case T match:
                return match;
            case TransformGroup group:
                foreach (var child in group.Children)
                {
                    var found = FindTransform<T>(child);
                    if (found is not null)
                    {
                        return found;
                    }
                }

                return null;
            default:
                return null;
        }
    }

    private static void ResetTranslate(FrameworkElement element)
    {
        if (FindTransform<TranslateTransform>(element) is TranslateTransform translate)
        {
            translate.X = 0;
            translate.Y = 0;
        }
    }

    private static void SetTranslateY(FrameworkElement element, double y)
    {
        if (FindTransform<TranslateTransform>(element) is TranslateTransform translate)
        {
            translate.Y = y;
        }
    }

    private static void ResetScale(FrameworkElement element)
    {
        if (FindTransform<ScaleTransform>(element) is ScaleTransform scale)
        {
            scale.ScaleX = 1;
            scale.ScaleY = 1;
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

    private static void TryInvoke(FrameworkElement element, Action<FrameworkElement> snap)
    {
        try
        {
            if (element.Dispatcher.CheckAccess())
            {
                snap(element);
            }
        }
        catch
        {
            // Last resort: leave the element as-is.
        }
    }
}
