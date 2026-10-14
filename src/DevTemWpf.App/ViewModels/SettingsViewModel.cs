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
        Reload();
    }

    public IReadOnlyList<ThemeMode> ThemeOptions { get; }

    public IReadOnlyList<ShellMode> ShellOptions { get; }

    [ObservableProperty]
    private ThemeMode selectedTheme = ThemeMode.System;

    [ObservableProperty]
    private bool backdropEnabled = true;

    [ObservableProperty]
    private ShellMode selectedShell = ShellMode.Sidebar;

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
        var s = _settings.Current;
        SelectedTheme = s.Theme;
        BackdropEnabled = s.BackdropEnabled;
        SelectedShell = s.Shell;
        ErrorMessage = null;
    }

    partial void OnSelectedThemeChanged(ThemeMode value) => _ = PersistAsync(s => s with { Theme = value });

    partial void OnBackdropEnabledChanged(bool value) => _ = PersistAsync(s => s with { BackdropEnabled = value });

    partial void OnSelectedShellChanged(ShellMode value) => _ = PersistAsync(s => s with { Shell = value });

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
