using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Resources;
using DevTemWpf.App.Services;
using DevTemWpf.App.Views;
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
        MenuItems = new ObservableCollection<object>
        {
            new NavigationViewItem(Strings.Nav_Home, SymbolRegular.Home24, typeof(HomePage)),
            new NavigationViewItem(Strings.Nav_Dashboard, SymbolRegular.Board24, typeof(DashboardPage)),
            new NavigationViewItem(Strings.Nav_Controls, SymbolRegular.DataHistogram24, typeof(ControlsPage)),
            new NavigationViewItem(Strings.Nav_Tokens, SymbolRegular.PaintBrush24, typeof(TokensPage)),
            new NavigationViewItem(Strings.Nav_Effects, SymbolRegular.Fluent24, typeof(EffectsPage)),
            new NavigationViewItem(Strings.Nav_Diagnostics, SymbolRegular.Shield24, typeof(DiagnosticsPage)),
        };
        FooterMenuItems = new ObservableCollection<object>
        {
            new NavigationViewItem(Strings.Nav_Settings, SymbolRegular.Settings24, typeof(SettingsPage)),
        };
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
        => shell == ShellMode.Rail
            ? NavigationViewPaneDisplayMode.LeftMinimal
            : NavigationViewPaneDisplayMode.Left;
}
