using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Resources;
using DevTemWpf.App.Services;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Effects demo ViewModel: policy/environment display, per-effect switches,
/// fallback status, and development-build profile info (no animation logic —
/// that lives in <see cref="IMotionService"/> / the page).
/// </summary>
public sealed partial class EffectsViewModel : ObservableObject, INavigationAware
{
    private static readonly bool ShowProfileDetail =
#if DEBUG
        true;
#else
        false;
#endif

    private readonly IMotionService _motion;
    private readonly ISettingsService _settings;
    private bool _reloading;

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

    [ObservableProperty]
    private string fallbackText = string.Empty;

    [ObservableProperty]
    private string profileDetailTitle = string.Empty;

    [ObservableProperty]
    private string profileDetailText = string.Empty;

    [ObservableProperty]
    private bool effectsEnabled = true;

    [ObservableProperty]
    private bool rippleEnabled = true;

    [ObservableProperty]
    private bool gradientEnabled = true;

    [ObservableProperty]
    private bool warpEnabled;

    public Task OnNavigatedToAsync()
    {
        Reload();
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    partial void OnEffectsEnabledChanged(bool value) => Persist(s => s with { EffectsEnabled = value });

    partial void OnRippleEnabledChanged(bool value) => Persist(s => s with { RippleEffectEnabled = value });

    partial void OnGradientEnabledChanged(bool value) => Persist(s => s with { GradientEffectEnabled = value });

    partial void OnWarpEnabledChanged(bool value) => Persist(s => s with { WarpEffectEnabled = value });

    private void Persist(Func<ShellSettings, ShellSettings> mutate)
    {
        if (_reloading)
        {
            return;
        }

        _ = PersistAsync(mutate);
    }

    private async Task PersistAsync(Func<ShellSettings, ShellSettings> mutate)
    {
        try
        {
            await _settings.UpdateAsync(mutate).ConfigureAwait(true);
            Reload();
        }
        catch
        {
            // A demo page never fails on a preference write.
        }
    }

    private void Reload()
    {
        _reloading = true;
        try
        {
            var current = _settings.Current;
            PolicyText = string.Format(Strings.Efx_PolicyFormat, _motion.CurrentPolicy, current.Motion);
            EnvironmentText = string.Format(
                Strings.Efx_EnvFormat,
                OnOff(_motion.OsReducedMotion),
                _motion.RenderTier,
                OnOff(current.EffectsEnabled));
            EffectsEnabled = current.EffectsEnabled;
            RippleEnabled = current.RippleEffectEnabled;
            GradientEnabled = current.GradientEffectEnabled;
            WarpEnabled = current.WarpEffectEnabled;
            FallbackText = string.Format(Strings.Efx_FallbackFormat, FallbackReason(current));
            ProfileDetailTitle = ShowProfileDetail ? Strings.Efx_ProfileTitle : string.Empty;
            ProfileDetailText = ShowProfileDetail
                ? string.Format(
                    Strings.Efx_ProfileDetailFormat,
                    (int)MotionTokens.Fast.TotalMilliseconds,
                    (int)MotionTokens.Standard.TotalMilliseconds,
                    (int)MotionTokens.Deliberate.TotalMilliseconds,
                    (int)MotionTokens.Stagger.TotalMilliseconds,
                    _motion.RenderTier)
                : string.Empty;
        }
        finally
        {
            _reloading = false;
        }
    }

    private string FallbackReason(ShellSettings current)
    {
        if (_motion.AllowsEffect(MotionEffect.Warp))
        {
            return Strings.Efx_FallbackActive;
        }

        if (_motion.OsReducedMotion)
        {
            return Strings.Efx_FallbackStaticReduced;
        }

        if (!current.EffectsEnabled)
        {
            return Strings.Efx_FallbackStaticEffects;
        }

        return MotionPolicy.AllowsDecorative(_motion.CurrentPolicy)
            ? Strings.Efx_FallbackStaticSwitch
            : Strings.Efx_FallbackStaticPolicy;
    }

    private static string OnOff(bool value) => value ? Strings.Efx_On : Strings.Efx_Off;
}
