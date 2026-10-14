using DevTemWpf.App.Services;
using Wpf.Ui.Controls;

namespace DevTemWpf.Unit;

public sealed class BackdropServiceTests
{
    private readonly BackdropService _service = new(isMicaSupported: () => true, isHighContrast: () => false);

    [Theory]
    [InlineData(true, false, true, WindowBackdropType.Mica)] // opted in, supported
    [InlineData(false, false, true, WindowBackdropType.None)] // user opted out
    [InlineData(true, true, true, WindowBackdropType.None)] // high contrast
    [InlineData(true, false, false, WindowBackdropType.None)] // unsupported GPU/DWM
    public void ResolveEffective_FallsBackToOpaque(
        bool enabled, bool highContrast, bool supported, WindowBackdropType expected)
    {
        Assert.Equal(expected, _service.ResolveEffective(enabled, highContrast, supported));
    }
}
