using DevTemWpf.App.Resources;
using DevTemWpf.App.Views;
using Wpf.Ui.Controls;

namespace DevTemWpf.App.Services;

/// <summary>
/// Stable navigation route identity shared by every shell (1.4.0).
/// The <c>Key</c> is the persistent identity used by the workspace
/// (documents, persistence), the command palette, and selection sync -
/// page <see cref="Type"/>s are never used as identities outside this file.
/// Titles resolve lazily through <see cref="Strings"/> so culture changes
/// apply the same way as the existing nav items. Conditional pages keep the
/// established preset guards so generated scaffolds stay exact.
/// </summary>
public sealed record NavigationRoute(string Key, Func<string> Title, SymbolRegular Icon, Type PageType, bool IsFooter = false);

/// <summary>Single source of navigable pages for shells, palette, and workspace.</summary>
public static class NavigationRoutes
{
    // NOTE: conditional entries use statement-level conditional blocks
    // (Add calls), never directives inside a collection expression, so the
    // template engine keeps the file intact (see 1.4.0 T08 evidence).
    public static IReadOnlyList<NavigationRoute> All { get; } = Build();

    public static IReadOnlyList<NavigationRoute> Menu =>
        All.Where(r => !r.IsFooter).ToList();

    public static IReadOnlyList<NavigationRoute> Footer =>
        All.Where(r => r.IsFooter).ToList();

    public static NavigationRoute? Find(string key)
        => All.FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.Ordinal));

    private static List<NavigationRoute> Build()
    {
        var routes = new List<NavigationRoute>
        {
            new("home", () => Strings.Nav_Home, SymbolRegular.Home24, typeof(HomePage)),
        };
        //#if (IncludeDashboard)
        routes.Add(new("dashboard", () => Strings.Nav_Dashboard, SymbolRegular.Board24, typeof(DashboardPage)));
        //#endif
        routes.Add(new("controls", () => Strings.Nav_Controls, SymbolRegular.DataHistogram24, typeof(ControlsPage)));
        routes.Add(new("data", () => Strings.Nav_Data, SymbolRegular.TableSimple24, typeof(DataPage)));
        routes.Add(new("tokens", () => Strings.Nav_Tokens, SymbolRegular.PaintBrush24, typeof(TokensPage)));
        routes.Add(new("effects", () => Strings.Nav_Effects, SymbolRegular.Fluent24, typeof(EffectsPage)));
        //#if (IncludeDiagnostics)
        routes.Add(new("diagnostics", () => Strings.Nav_Diagnostics, SymbolRegular.Shield24, typeof(DiagnosticsPage)));
        //#endif
        routes.Add(new("settings", () => Strings.Nav_Settings, SymbolRegular.Settings24, typeof(SettingsPage), IsFooter: true));
        return routes;
    }
}
