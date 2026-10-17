using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Services;
using Microsoft.Extensions.Options;
using Wpf.Ui.Controls;

namespace DevTemWpf.App;

/// <summary>Shell ViewModel: navigation items + pane state (ADR-0005, ADR-0015).</summary>
public sealed partial class MainViewModel : ObservableObject
{
    public const double CompactWidthThreshold = 800;

    /// <summary>
    /// Shell applied to the live chrome (launch value). A persisted shell
    /// change takes effect at next launch (ADR-0015): WPF-UI 4.3.0 crashes
    /// when PaneDisplayMode is swapped on a live NavigationView (NaN
    /// DoubleAnimationException during re-template; upstream PR #1012/#1015
    /// fixed only the load path — proven by UI runs in 1.4.0). The settings
    /// page documents the restart requirement next to the selector.
    /// </summary>
    private readonly ShellMode _appliedShell;

    public MainViewModel(ISettingsService settings, IOptions<AppOptions> options)
    {
        AppTitle = options.Value.Title;
        MenuItems = new ObservableCollection<object>(
            NavigationRoutes.Menu.Select(r => new NavigationViewItem(r.Title(), r.Icon, r.PageType)));
        FooterMenuItems = new ObservableCollection<object>(
            NavigationRoutes.Footer.Select(r => new NavigationViewItem(r.Title(), r.Icon, r.PageType)));
        _appliedShell = settings.Current.Shell;
        RefreshShell();
    }

    public ObservableCollection<object> MenuItems { get; }

    public ObservableCollection<object> FooterMenuItems { get; }

    public string AppTitle { get; }

    [ObservableProperty]
    private NavigationViewPaneDisplayMode paneMode = NavigationViewPaneDisplayMode.Left;

    [ObservableProperty]
    private bool isPaneOpen = true;

    /// <summary>Narrow windows collapse the pane; widening restores the applied shell.</summary>
    public void ApplyResponsiveWidth(double width)
    {
        if (width < CompactWidthThreshold)
        {
            IsPaneOpen = false;
            return;
        }

        RefreshShell();
    }

    private void RefreshShell()
    {
        PaneMode = MapMode(_appliedShell);
        IsPaneOpen = _appliedShell == ShellMode.Sidebar;
    }

    private static NavigationViewPaneDisplayMode MapMode(ShellMode shell)
        => shell switch
        {
            ShellMode.Rail => NavigationViewPaneDisplayMode.LeftMinimal,
            ShellMode.Top => NavigationViewPaneDisplayMode.Top,
            ShellMode.Tabbed => NavigationViewPaneDisplayMode.LeftMinimal,
            _ => NavigationViewPaneDisplayMode.Left,
        };
}
