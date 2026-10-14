using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DevTemWpf.App.Services;

/// <summary>
/// Thin seam over WPF-UI statics so theme logic is unit-testable without a
/// running <c>Application</c> (ADR-0006).
/// </summary>
public interface IThemeApplier
{
    void Apply(ApplicationTheme theme, WindowBackdropType backdrop);

    /// <summary>Current OS theme mapped to Light/Dark/HighContrast.</summary>
    ApplicationTheme GetSystemApplicationTheme();

    bool IsHighContrast();
}
