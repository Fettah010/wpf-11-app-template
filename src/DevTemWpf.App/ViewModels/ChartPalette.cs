using SkiaSharp;
using MediaColor = System.Windows.Media.Color;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Resolved chart colors for one theme snapshot. Pure mapping (unit-tested):
/// light text means a dark theme and vice versa, so any theme — including
/// high-contrast and custom accents — flows through automatically.
/// SkiaSharp is used via the LiveCharts transitive closure (no new package).
/// </summary>
public sealed record ChartPalette(SKColor AxisText, SKColor GridLines, SKColor Series, bool Dark)
{
    public static ChartPalette For(MediaColor text, MediaColor accent)
    {
        var dark = Luminance(text) > 0.5;
        var gridBase = dark ? SKColors.White : SKColors.Black;
        var grid = new SKColor(gridBase.Red, gridBase.Green, gridBase.Blue, 40);
        return new ChartPalette(
            new SKColor(text.R, text.G, text.B, text.A),
            grid,
            new SKColor(accent.R, accent.G, accent.B, accent.A),
            dark);
    }

    public static double Luminance(MediaColor color) =>
        (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;
}
