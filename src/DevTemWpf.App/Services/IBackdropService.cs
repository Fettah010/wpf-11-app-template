using Wpf.Ui.Controls;

namespace DevTemWpf.App.Services;

/// <summary>Maps settings to an effective window backdrop (pure logic).</summary>
public interface IBackdropService
{
    WindowBackdropType ResolveEffective(bool enabled, bool isHighContrast, bool isMicaSupported);

    /// <summary>Effective backdrop for the current environment.</summary>
    WindowBackdropType Current(bool enabled);
}
