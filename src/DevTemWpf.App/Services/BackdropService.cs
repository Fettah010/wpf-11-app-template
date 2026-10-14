using Wpf.Ui.Controls;

namespace DevTemWpf.App.Services;

/// <summary>
/// Mica by default; opaque <see cref="WindowBackdropType.None"/> whenever the
/// user opts out, Mica is unsupported, or high contrast is active (ADR-0006).
/// </summary>
public sealed class BackdropService : IBackdropService
{
    private readonly Func<bool> _isMicaSupported;
    private readonly Func<bool> _isHighContrast;

    public BackdropService(Func<bool>? isMicaSupported = null, Func<bool>? isHighContrast = null)
    {
        _isMicaSupported = isMicaSupported ?? (() => WindowBackdrop.IsSupported(WindowBackdropType.Mica));
        _isHighContrast = isHighContrast ?? (() => Wpf.Ui.Appearance.ApplicationThemeManager.IsHighContrast());
    }

    public WindowBackdropType ResolveEffective(bool enabled, bool isHighContrast, bool isMicaSupported)
    {
        if (!enabled || isHighContrast || !isMicaSupported)
        {
            return WindowBackdropType.None;
        }

        return WindowBackdropType.Mica;
    }

    /// <summary>Effective backdrop for the current environment.</summary>
    public WindowBackdropType Current(bool enabled)
        => ResolveEffective(enabled, _isHighContrast(), _isMicaSupported());
}
