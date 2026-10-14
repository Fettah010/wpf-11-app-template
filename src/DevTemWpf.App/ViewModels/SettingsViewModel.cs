using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Services;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Settings page ViewModel: live-apply + persist shell preferences.</summary>
public sealed partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private readonly ISettingsService _settings;
    private readonly IThemeService _theme;

    public SettingsViewModel(ISettingsService settings, IThemeService theme)
    {
        _settings = settings;
        _theme = theme;
        ThemeOptions = [ThemeMode.System, ThemeMode.Light, ThemeMode.Dark];
        ShellOptions = [ShellMode.Sidebar, ShellMode.Rail];
        DensityOptions = [DensityMode.Comfortable, DensityMode.Compact];
        Reload();
    }

    public IReadOnlyList<ThemeMode> ThemeOptions { get; }

    public IReadOnlyList<ShellMode> ShellOptions { get; }

    public IReadOnlyList<DensityMode> DensityOptions { get; }

    [ObservableProperty]
    private ThemeMode selectedTheme = ThemeMode.System;

    [ObservableProperty]
    private bool backdropEnabled = true;

    [ObservableProperty]
    private ShellMode selectedShell = ShellMode.Sidebar;

    [ObservableProperty]
    private DensityMode selectedDensity = DensityMode.Comfortable;

    [ObservableProperty]
    private bool useCustomAccent;

    [ObservableProperty]
    private string accentHex = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    public Task OnNavigatedToAsync()
    {
        Reload();
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    private void Reload()
    {
        _reloading = true;
        try
        {
            var s = _settings.Current;
            SelectedTheme = s.Theme;
            BackdropEnabled = s.BackdropEnabled;
            SelectedShell = s.Shell;
            SelectedDensity = s.Density;
            UseCustomAccent = s.AccentOverride is not null;
            AccentHex = s.AccentOverride ?? string.Empty;
            ErrorMessage = null;
        }
        finally
        {
            _reloading = false;
        }
    }

    private bool _reloading;

    private bool PersistGuard() => _reloading;

    partial void OnSelectedThemeChanged(ThemeMode value)
    {
        if (!PersistGuard()) _ = PersistAsync(s => s with { Theme = value });
    }

    partial void OnBackdropEnabledChanged(bool value)
    {
        if (!PersistGuard()) _ = PersistAsync(s => s with { BackdropEnabled = value });
    }

    partial void OnSelectedShellChanged(ShellMode value)
    {
        if (!PersistGuard()) _ = PersistAsync(s => s with { Shell = value });
    }

    partial void OnSelectedDensityChanged(DensityMode value)
    {
        if (!PersistGuard()) _ = PersistAsync(s => s with { Density = value });
    }

    partial void OnUseCustomAccentChanged(bool value)
    {
        if (!PersistGuard()) _ = PersistAccentAsync();
    }

    partial void OnAccentHexChanged(string value)
    {
        if (!PersistGuard()) _ = PersistAccentAsync();
    }

    private async Task PersistAccentAsync()
    {
        if (!UseCustomAccent)
        {
            await PersistAsync(s => s with { AccentOverride = (string?)null }).ConfigureAwait(true);
            return;
        }

        if (!IsValidHex(AccentHex))
        {
            ErrorMessage = "Accent must be #RRGGBB or #AARRGGBB.";
            return;
        }

        await PersistAsync(s => s with { AccentOverride = AccentHex.Trim() }).ConfigureAwait(true);
    }

    /// <summary>Hex validation shared with tests (VM surface stays UI-free).</summary>
    public static bool IsValidHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return false;
        }

        hex = hex.Trim().TrimStart('#');
        return (hex.Length == 6 || hex.Length == 8)
            && uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out _);
    }

    private async Task PersistAsync(Func<ShellSettings, ShellSettings> mutate)
    {
        try
        {
            ErrorMessage = null;
            await _settings.UpdateAsync(mutate).ConfigureAwait(true);
            await _theme.ApplyCurrentAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
