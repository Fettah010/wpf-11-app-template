using DevTemWpf.App.Services;
using DevTemWpf.App.Views;

namespace DevTemWpf.Unit;

/// <summary>Route registry contract (1.4.0): stable keys shared by all shells.</summary>
public sealed class NavigationRoutesTests
{
    [Fact]
    public void All_Keys_AreUnique_AndCoverEveryPage()
    {
        var keys = NavigationRoutes.All.Select(r => r.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            new HashSet<string>(
                [
                    "home",
                    //#if (IncludeDashboard)
                    "dashboard",
                    //#endif
                    "controls",
                    "data",
                    "tokens",
                    "effects",
                    //#if (IncludeDiagnostics)
                    "diagnostics",
                    //#endif
                    "settings",
                ]),
            keys.ToHashSet(StringComparer.Ordinal));
    }

    [Fact]
    public void Find_ReturnsRoute_AndNullForUnknown()
    {
        Assert.Equal(typeof(HomePage), NavigationRoutes.Find("home")?.PageType);
        Assert.Null(NavigationRoutes.Find("nope"));
        Assert.Null(NavigationRoutes.Find("Home"));
    }

    [Fact]
    public void Titles_ResolveNonEmpty_AndFooterSplitIsExact()
    {
        foreach (var route in NavigationRoutes.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(route.Title()));
        }

        Assert.Single(NavigationRoutes.Footer);
        Assert.Equal("settings", NavigationRoutes.Footer[0].Key);
        Assert.DoesNotContain(NavigationRoutes.Menu, r => r.IsFooter);
    }

    [Fact]
    public void OldSettings_WithLegacyShell_LoadsUnchanged()
    {
        // Additive enum members must not break persisted 1.x settings files.
        Assert.Equal(ShellMode.Sidebar, new ShellSettings { Shell = ShellMode.Sidebar }.Shell);
        Assert.Equal(2, (int)ShellMode.Top);
        Assert.Equal(3, (int)ShellMode.Tabbed);
    }
}
