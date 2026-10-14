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
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(ControlsPage));
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(TokensPage));
        Assert.Contains(vm.MenuItems.OfType<NavigationViewItem>(), i => i.TargetPageType == typeof(EffectsPage));
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
