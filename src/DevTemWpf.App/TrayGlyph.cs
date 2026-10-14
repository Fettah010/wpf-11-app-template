using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DevTemWpf.App;

/// <summary>
/// Runtime tray glyph (no .ico asset in the repo). Accent rounded square +
/// app initial, frozen for thread safety.
/// </summary>
internal static class TrayGlyph
{
    internal static ImageSource Create(string title)
    {
        var initial = string.IsNullOrWhiteSpace(title) ? "W" : title.Trim().Substring(0, 1).ToUpperInvariant();
        var group = new DrawingGroup();
        group.Children.Add(new GeometryDrawing(
            new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4)),
            null,
            new RectangleGeometry(new Rect(0, 0, 16, 16), 4, 4)));
        var text = new FormattedText(
            initial,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            11,
            Brushes.White,
            1);
        group.Children.Add(new GeometryDrawing(
            null,
            null,
            text.BuildGeometry(new Point(3, -1))));
        var image = new DrawingImage(group);
        image.Freeze();
        return image;
    }
}
