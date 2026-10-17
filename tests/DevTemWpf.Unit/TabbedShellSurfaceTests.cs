using System.Text.RegularExpressions;

namespace DevTemWpf.Unit;

/// <summary>
/// 1.4.1 tabbed-shell surface guard. The Tabbed shell hosts documents in a
/// WPF-UI <c>ui:TabView</c>, which is an empty
/// <see cref="System.Windows.Controls.TabControl"/> subclass. WPF does not
/// apply WPF-UI's implicit <c>Style TargetType="TabControl"</c> to a derived
/// type, so without an explicit style the built-in WPF template paints a white
/// content border (measured 67.9% pure-white pixels in the dark theme from the
/// shipped 1.4.0 shell). These assertions keep the themed style wired and fail
/// loudly if it is removed again. Repository-only (excluded from the template).
/// </summary>
public sealed class TabbedShellSurfaceTests
{
    private static string ReadWorkspacePageXaml()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        var path = Path.Combine(dir.FullName, "src", "DevTemWpf.App", "Views", "WorkspacePage.xaml");
        Assert.True(File.Exists(path), "WorkspacePage.xaml not found — run from a repo checkout.");
        return File.ReadAllText(path);
    }

    // Opening element only: attributes may span lines but never contain '>'.
    private static string OpeningElement(string xaml, string tag)
    {
        var match = Regex.Match(xaml, $"<{tag}\\b[^>]*>", RegexOptions.Singleline);
        Assert.True(match.Success, $"WorkspacePage must declare a <{tag}> element.");
        return match.Value;
    }

    [Fact]
    public void TabView_ExplicitlyAppliesWpfUiTabControlStyle()
    {
        // Regression guard for the 1.4.0 white document area: the derived
        // ui:TabView must opt into WPF-UI's TabControl style, otherwise the
        // built-in white template (and its 67.9% white surface) returns.
        var tag = OpeningElement(ReadWorkspacePageXaml(), "ui:TabView");
        Assert.Matches(@"Style\s*=\s*""\{StaticResource \{x:Type TabControl\}\}""", tag);
    }

    [Fact]
    public void DetailSplitter_UsesThemedDividerBrush()
    {
        // The default GridSplitter rendered bright #F0F0F0 in dark theme; it
        // must use the semantic divider token instead.
        var tag = OpeningElement(ReadWorkspacePageXaml(), "GridSplitter");
        Assert.Matches(@"Background\s*=\s*""\{DynamicResource AppDividerBrush\}""", tag);
    }
}
