using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DevTemWpf.App.Services;

/// <summary>Production <see cref="IThemeApplier"/> over WPF-UI statics.</summary>
public sealed class WpfUiThemeApplier : IThemeApplier
{
    public void Apply(ApplicationTheme theme, WindowBackdropType backdrop)
        => ApplicationThemeManager.Apply(theme, backdrop);

    public void ApplySystemTheme() => ApplicationThemeManager.ApplySystemTheme();

    public bool IsHighContrast() => ApplicationThemeManager.IsHighContrast();
}
