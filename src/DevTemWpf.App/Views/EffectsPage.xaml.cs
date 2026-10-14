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
/// decisions live in <see cref="IMotionService"/>.
/// </summary>
public partial class EffectsPage : Page, INavigableView<EffectsViewModel>
{
    private readonly IMotionService _motion;
    private Storyboard? _gradientLoop;

    public EffectsPage(EffectsViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        _motion = motion;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = _motion.PlayEntranceAsync(this);
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
        _ = _motion.PlayEntranceAsync(PresetPreview, duration);
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
            if (_motion.CurrentPolicy != EffectiveMotion.Full)
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
        }
        catch
        {
            GradientToggle.IsChecked = false;
        }
    }

    private void StopGradient()
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
