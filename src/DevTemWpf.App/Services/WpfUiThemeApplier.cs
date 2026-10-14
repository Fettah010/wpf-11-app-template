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

    public void ApplyAccentOverride(string? hex, ApplicationTheme theme)
    {
        if (TryParseHex(hex, out var color))
        {
            ApplicationAccentColorManager.Apply(color, theme);
            return;
        }

        ApplicationAccentColorManager.ApplySystemAccent();
    }

    private static bool TryParseHex(string? hex, out System.Windows.Media.Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(hex))
        {
            return false;
        }

        hex = hex.Trim().TrimStart('#');
        if (hex.Length != 6 && hex.Length != 8)
        {
            return false;
        }

        if (!uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var value))
        {
            return false;
        }

        color = hex.Length == 6
            ? System.Windows.Media.Color.FromRgb((byte)(value >> 16), (byte)(value >> 8), (byte)value)
            : System.Windows.Media.Color.FromArgb((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
        return true;
    }
}
