using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DevTemWpf.App.Services;

/// <summary>Production <see cref="IThemeApplier"/> over WPF-UI statics.</summary>
public sealed class WpfUiThemeApplier : IThemeApplier
{
    public void Apply(ApplicationTheme theme, WindowBackdropType backdrop)
        => ApplicationThemeManager.Apply(theme, backdrop);

    public ApplicationTheme GetSystemApplicationTheme()
        => ApplicationThemeManager.GetSystemTheme() switch
        {
            SystemTheme.Dark or SystemTheme.CapturedMotion or SystemTheme.Glow => ApplicationTheme.Dark,
            SystemTheme.HC1 or SystemTheme.HC2 or SystemTheme.HCBlack or SystemTheme.HCWhite => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Light,
        };

    public bool IsHighContrast() => ApplicationThemeManager.IsHighContrast();
}
