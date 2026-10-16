using DevTemWpf.App.Resources;
using DevTemWpf.App.Views;
using Wpf.Ui.Controls;

namespace DevTemWpf.App.Services;

/// <summary>
/// Stable navigation route identity shared by every shell (1.4.0).
/// The <c>Key</c> is the persistent identity used by the workspace
/// (documents, persistence), the command palette, and selection sync —
/// page <see cref="Type"/>s are never used as identities outside this file.
/// Titles resolve lazily through <see cref="Strings"/> so culture changes
/// apply the same way as the existing nav items. Conditional pages keep the
/// established <c>//#if</c> preset guards so generated scaffolds stay exact.
/// </summary>
public sealed record NavigationRoute(string Key, Func<string> Title, SymbolRegular Icon, Type PageType, bool IsFooter = false);

/// <summary>Single source of navigable pages for shells, palette, and workspace.</summary>
public static class NavigationRoutes
{
    public static IReadOnlyList<NavigationRoute> All { get; } =
    [
        new("home", () => Strings.Nav_Home, SymbolRegular.Home24, typeof(HomePage)),
        //#if (IncludeDashboard)
        new("dashboard", () => Strings.Nav_Dashboard, SymbolRegular.Board24, typeof(DashboardPage)),
        //#endif
        new("controls", () => Strings.Nav_Controls, SymbolRegular.DataHistogram24, typeof(ControlsPage)),
        new("data", () => Strings.Nav_Data, SymbolRegular.TableSimple24, typeof(DataPage)),
        new("tokens", () => Strings.Nav_Tokens, SymbolRegular.PaintBrush24, typeof(TokensPage)),
        new("effects", () => Strings.Nav_Effects, SymbolRegular.Fluent24, typeof(EffectsPage)),
        //#if (IncludeDiagnostics)
        new("diagnostics", () => Strings.Nav_Diagnostics, SymbolRegular.Shield24, typeof(DiagnosticsPage)),
        //#endif
        new("settings", () => Strings.Nav_Settings, SymbolRegular.Settings24, typeof(SettingsPage), IsFooter: true),
    ];

    public static IReadOnlyList<NavigationRoute> Menu =>
        All.Where(r => !r.IsFooter).ToList();

    public static IReadOnlyList<NavigationRoute> Footer =>
        All.Where(r => r.IsFooter).ToList();

    public static NavigationRoute? Find(string key)
        => All.FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.Ordinal));
}
