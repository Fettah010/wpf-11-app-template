using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Resources;
using DevTemWpf.App.Services;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Effects demo ViewModel: policy/environment display (no animation logic).</summary>
public sealed partial class EffectsViewModel : ObservableObject, INavigationAware
{
    private readonly IMotionService _motion;
    private readonly ISettingsService _settings;

    public EffectsViewModel(IMotionService motion, ISettingsService settings)
    {
        _motion = motion;
        _settings = settings;
        Reload();
    }

    [ObservableProperty]
    private string policyText = string.Empty;

    [ObservableProperty]
    private string environmentText = string.Empty;

    public Task OnNavigatedToAsync()
    {
        Reload();
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    private void Reload()
    {
        var current = _settings.Current;
        PolicyText = string.Format(Strings.Efx_PolicyFormat, _motion.CurrentPolicy, current.Motion);
        EnvironmentText = string.Format(
            Strings.Efx_EnvFormat,
            _motion.OsReducedMotion ? Strings.Efx_Off : Strings.Efx_On,
            _motion.RenderTier,
            current.EffectsEnabled ? Strings.Efx_On : Strings.Efx_Off);
    }
}
