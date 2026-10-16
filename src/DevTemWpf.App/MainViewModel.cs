using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Services;
using Microsoft.Extensions.Options;
using Wpf.Ui.Controls;

namespace DevTemWpf.App;

/// <summary>Shell ViewModel: navigation items + pane state (ADR-0005).</summary>
public sealed partial class MainViewModel : ObservableObject
{
    public const double CompactWidthThreshold = 800;

    private readonly ISettingsService _settings;

    public MainViewModel(ISettingsService settings, IOptions<AppOptions> options)
    {
        _settings = settings;
        AppTitle = options.Value.Title;
        MenuItems = new ObservableCollection<object>(
            NavigationRoutes.Menu.Select(r => new NavigationViewItem(r.Title(), r.Icon, r.PageType)));
        FooterMenuItems = new ObservableCollection<object>(
            NavigationRoutes.Footer.Select(r => new NavigationViewItem(r.Title(), r.Icon, r.PageType)));
        RefreshShell(_settings.Current);
        _settings.Updated += (_, s) => RefreshShell(s);
    }

    public ObservableCollection<object> MenuItems { get; }

    public ObservableCollection<object> FooterMenuItems { get; }

    public string AppTitle { get; }

    [ObservableProperty]
    private NavigationViewPaneDisplayMode paneMode = NavigationViewPaneDisplayMode.Left;

    [ObservableProperty]
    private bool isPaneOpen = true;

    /// <summary>Narrow windows collapse the pane; widening restores the preference.</summary>
    public void ApplyResponsiveWidth(double width)
    {
        if (width < CompactWidthThreshold)
        {
            IsPaneOpen = false;
            return;
        }

        RefreshShell(_settings.Current);
    }

    private void RefreshShell(ShellSettings s)
    {
        PaneMode = MapMode(s.Shell);
        IsPaneOpen = s.Shell == ShellMode.Sidebar;
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
