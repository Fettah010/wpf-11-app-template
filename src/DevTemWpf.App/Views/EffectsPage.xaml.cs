using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>
/// Effects showcase (view wiring + demo orchestration only). All policy
/// decisions live in <see cref="IMotionService"/>; lifecycle cleanup is owned
/// by <see cref="MotionScope"/>.
/// </summary>
public partial class EffectsPage : Page, INavigableView<EffectsViewModel>
{
    private readonly IMotionService _motion;
    private readonly MotionScope _scope;
    private Storyboard? _gradientLoop;
    private IDisposable? _gradientRegistration;

    public EffectsPage(EffectsViewModel viewModel, IMotionService motion, MotionScope scope)
    {
        ViewModel = viewModel;
        _motion = motion;
        _scope = scope;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = _motion.PlayEntranceAsync(this, cancellationToken: _scope.Token);
        Unloaded += (_, _) => StopGradient();
    }

    public EffectsViewModel ViewModel { get; }

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

    private void OnGradientToggled(object sender, RoutedEventArgs e)
    {
        if (GradientToggle.IsChecked == true)
        {
            StartGradient();
        }
        else
        {
            StopGradient();
        }
    }

    private void StartGradient()
    {
        StopGradient();
        try
        {
            if (!_motion.AllowsEffect(MotionEffect.Gradient))
            {
                GradientToggle.IsChecked = false;
                return;
            }

            var loop = new Storyboard
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
            };
            var slide = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(3));
            Storyboard.SetTarget(slide, GradientStopA);
            Storyboard.SetTargetProperty(slide, new PropertyPath(System.Windows.Media.GradientStop.OffsetProperty));
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
            GradientToggle.IsChecked = false;
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
}
