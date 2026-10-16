using DevTemWpf.App;
using DevTemWpf.App.Services;
using DevTemWpf.App.Views;
using Microsoft.Extensions.Options;
using Wpf.Ui.Controls;

namespace DevTemWpf.Unit;

public sealed class MainViewModelTests : IDisposable
{
    private readonly string _settingsPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { File.Delete(_settingsPath); } catch { /* best effort */ }
    }

    private MainViewModel Create(ShellSettings? seed = null)
    {
        var settings = new SettingsService(_settingsPath);
        if (seed is not null)
        {
            settings.UpdateAsync(_ => seed, TestContext.Current.CancellationToken).GetAwaiter().GetResult();
        }

        return new MainViewModel(settings, Options.Create(new AppOptions()));
    }

    [WpfFact]
    public void Menu_ContainsHome_And_FooterContainsSettings()
    {
        var vm = Create();

        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(HomePage));
        //#if (IncludeDashboard)
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(DashboardPage));
        //#endif
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(ControlsPage));
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(TokensPage));
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(EffectsPage));
        //#if (IncludeDiagnostics)
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(DiagnosticsPage));
        //#endif
        Assert.Contains(vm.FooterMenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(SettingsPage));
    }

    [WpfFact]
    public void Constructor_ExposesConfiguredAppTitle()
    {
        var vm = Create();

        Assert.Equal(new AppOptions().Title, vm.AppTitle);
    }

    [WpfFact]
    public void SidebarDefault_UsesLeftPane_Open()
    {
        var vm = Create(new ShellSettings { Shell = ShellMode.Sidebar });

        Assert.Equal(NavigationViewPaneDisplayMode.Left, vm.PaneMode);
        Assert.True(vm.IsPaneOpen);
    }

    [WpfFact]
    public void Rail_UsesMinimalPane()
    {
        var vm = Create(new ShellSettings { Shell = ShellMode.Rail });

        Assert.Equal(NavigationViewPaneDisplayMode.LeftMinimal, vm.PaneMode);
    }

    [WpfFact]
    public void Top_UsesTopPane()
    {
        var vm = Create(new ShellSettings { Shell = ShellMode.Top });

        Assert.Equal(NavigationViewPaneDisplayMode.Top, vm.PaneMode);
    }

    [WpfFact]
    public void Tabbed_UsesMinimalRailChrome()
    {
        var vm = Create(new ShellSettings { Shell = ShellMode.Tabbed });

        Assert.Equal(NavigationViewPaneDisplayMode.LeftMinimal, vm.PaneMode);
        Assert.False(vm.IsPaneOpen);
    }

    [WpfFact]
    public void MenuItems_MatchRegistry_InOrder()
    {
        var vm = Create();

        var expectedMenu = DevTemWpf.App.Services.NavigationRoutes.Menu.Select(r => r.PageType).ToList();
        var actualMenu = vm.MenuItems.OfType<NavigationViewItem>().Select(i => i.TargetPageType!).ToList();
        Assert.Equal(expectedMenu, actualMenu);

        var expectedFooter = DevTemWpf.App.Services.NavigationRoutes.Footer.Select(r => r.PageType).ToList();
        var actualFooter = vm.FooterMenuItems.OfType<NavigationViewItem>().Select(i => i.TargetPageType!).ToList();
        Assert.Equal(expectedFooter, actualFooter);
    }

    [WpfFact]
    public void NarrowWidth_CollapsesPane_And_Widening_Restores()
    {
        var vm = Create(new ShellSettings { Shell = ShellMode.Sidebar });

        vm.ApplyResponsiveWidth(600);
        Assert.False(vm.IsPaneOpen);

        vm.ApplyResponsiveWidth(1200);
        Assert.True(vm.IsPaneOpen);
        Assert.Equal(NavigationViewPaneDisplayMode.Left, vm.PaneMode);
    }
}
