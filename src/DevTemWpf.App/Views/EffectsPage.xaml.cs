using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>
/// Effects showcase (view wiring + demo orchestration only). Policy decisions
/// live in <see cref="IMotionService"/>; loop cleanup is owned by
/// <see cref="MotionScope"/>. The warp prototype is a backdrop-only simulated
/// distortion (no text or controls inside), off by default, with a static
/// fallback when disabled.
/// </summary>
public partial class EffectsPage : Page, INavigableView<EffectsViewModel>
{
    private readonly IMotionService _motion;
    private readonly MotionScope _scope;
    private Storyboard? _gradientLoop;
    private Storyboard? _warpLoop;
    private IDisposable? _gradientRegistration;
    private IDisposable? _warpRegistration;

    public EffectsPage(EffectsViewModel viewModel, IMotionService motion, MotionScope scope)
    {
        ViewModel = viewModel;
        _motion = motion;
        _scope = scope;
        DataContext = this;
        InitializeComponent();
        ViewModel.PropertyChanged += OnViewModelChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public EffectsViewModel ViewModel { get; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = _motion.PlayEntranceAsync(this, cancellationToken: _scope.Token);
        SyncEffectLoops();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopGradient();
        StopWarp();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EffectsViewModel.EffectsEnabled)
            or nameof(EffectsViewModel.GradientEnabled)
            or nameof(EffectsViewModel.WarpEnabled))
        {
            SyncEffectLoops();
        }
    }

    private void OnPresetClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string preset })
        {
            return;
        }

        var duration = preset switch
        {
            "Subtle" => MotionTokens.Fast,
            "Expressive" => MotionTokens.Deliberate,
            _ => MotionTokens.Standard,
        };
        _ = _motion.PlayEntranceAsync(PresetPreview, duration, _scope.Token);
    }

    private void OnRippleSurfaceDown(object sender, MouseButtonEventArgs e)
    {
        _motion.PlayRipple(RippleHost, e.GetPosition(RippleHost));
    }

    /// <summary>
    /// Effective (not just preferred) state, computed from the switches and the
    /// resolved policy so the loops react immediately — before the settings
    /// write round-trips.
    /// </summary>
    private bool EffectActive(MotionEffect effect)
    {
        if (!ViewModel.EffectsEnabled || !MotionPolicy.AllowsDecorative(_motion.CurrentPolicy))
        {
            return false;
        }

        return effect switch
        {
            MotionEffect.Ripple => ViewModel.RippleEnabled,
            MotionEffect.Gradient => ViewModel.GradientEnabled,
            MotionEffect.Warp => ViewModel.WarpEnabled,
            _ => false,
        };
    }

    private void SyncEffectLoops()
    {
        if (EffectActive(MotionEffect.Gradient))
        {
            if (_gradientLoop is null)
            {
                StartGradient();
            }
        }
        else
        {
            StopGradient();
        }

        if (EffectActive(MotionEffect.Warp))
        {
            if (_warpLoop is null)
            {
                StartWarp();
            }
        }
        else
        {
            StopWarp();
        }
    }

    private void StartGradient()
    {
        StopGradient();
        try
        {
            var loop = new Storyboard
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
            };
            var slide = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(3));
            Storyboard.SetTarget(slide, GradientStopA);
            Storyboard.SetTargetProperty(slide, new PropertyPath(GradientStop.OffsetProperty));
            loop.Children.Add(slide);
            // Target is set explicitly (GradientStop is not an element), so
            // the parameterless Begin is required here.
            loop.Begin();
            _gradientLoop = loop;
            // Deactivation / shutdown / theme change stop it via the shared scope.
            _gradientRegistration = _scope.Register(StopGradientCore);
        }
        catch
        {
            _gradientLoop = null;
        }
    }

    private void StopGradient()
    {
        _gradientRegistration?.Dispose();
        _gradientRegistration = null;
        StopGradientCore();
    }

    private void StopGradientCore()
    {
        try
        {
            _gradientLoop?.Stop();
        }
        catch
        {
            // Best effort.
        }
        finally
        {
            _gradientLoop = null;
        }
    }

    private void StartWarp()
    {
        StopWarp();
        try
        {
            // Simulated distortion: the backdrop's radial highlight drifts
            // across the surface. No HLSL, no render-target effect, so text
            // clarity and hit-testing are untouched (ADR-0016).
            var loop = new Storyboard
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
            };
            var duration = TimeSpan.FromSeconds(4);
            AddWarpAnimation(loop, RadialGradientBrush.CenterProperty, duration);
            AddWarpAnimation(loop, RadialGradientBrush.GradientOriginProperty, duration);
            loop.Begin();
            _warpLoop = loop;
            _warpRegistration = _scope.Register(StopWarpCore);
        }
        catch
        {
            _warpLoop = null;
        }
    }

    private void AddWarpAnimation(Storyboard loop, DependencyProperty property, TimeSpan duration)
    {
        var drift = new PointAnimation(new Point(0.25, 0.5), new Point(0.75, 0.5), duration)
        {
            EasingFunction = MotionTokens.EaseInOutCubic(),
        };
        Storyboard.SetTarget(drift, WarpBrush);
        Storyboard.SetTargetProperty(drift, new PropertyPath(property));
        loop.Children.Add(drift);
    }

    private void StopWarp()
    {
        _warpRegistration?.Dispose();
        _warpRegistration = null;
        StopWarpCore();
    }

    private void StopWarpCore()
    {
        try
        {
            _warpLoop?.Stop();
        }
        catch
        {
            // Best effort.
        }
        finally
        {
            _warpLoop = null;
        }
    }
}
