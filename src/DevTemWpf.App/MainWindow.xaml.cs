using System.Windows;
using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.Views;
using Microsoft.Extensions.Options;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;

namespace DevTemWpf.App;

/// <summary>
/// Shell wiring only: navigation attach, responsive collapse, window chrome.
/// No business logic here (protocol rule 7).
/// </summary>
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly ISettingsService _settings;
    private readonly Wpf.Ui.Tray.INotifyIconService _tray;

    public MainWindow(
        MainViewModel viewModel,
        IOptions<AppOptions> options,
        INavigationViewPageProvider pageProvider,
        Wpf.Ui.ISnackbarService snackbar,
        Wpf.Ui.IContentDialogService dialogs,
        Wpf.Ui.Tray.INotifyIconService tray,
        ISettingsService settings,
        IBackdropService backdrop)
    {
        _settings = settings;
        _tray = tray;

        // Density before init so first layout uses the persisted value.
        DesignTokenBootstrap.ApplyDensity(settings.Current.Density);

        // Set before InitializeComponent so FluentWindow picks it up at
        // OnSourceInitialized (its own timing). Never re-set afterwards:
        // re-setting the DP post-init crashes in SetWindowChrome on .NET 10.
        // Runtime material changes go through ApplicationThemeManager.Apply
        // (DWM-only path, no WindowChrome touch).
        WindowBackdropType = backdrop.Current(settings.Current.BackdropEnabled);

        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;
        Title = options.Value.Title;
        Width = options.Value.Width;
        Height = options.Value.Height;

        RootNavigation.SetPageProviderService(pageProvider);
        snackbar.SetSnackbarPresenter(RootSnackbar);
        dialogs.SetDialogHost(RootDialogHost);
        SetupTray(options.Value.Title);

        SizeChanged += (_, e) => ViewModel.ApplyResponsiveWidth(e.NewSize.Width);
        Loaded += (_, _) => OnFirstLoaded();
        _settings.Updated += (_, s) => DesignTokenBootstrap.ApplyDensity(s.Density);
    }

    public MainViewModel ViewModel { get; }

    /// <summary>Tray: icon exists only while minimize-to-tray is enabled.</summary>
    private void SetupTray(string title)
    {
        _tray.SetParentWindow(this);
        _tray.TooltipText = title;
        _tray.Icon = TrayGlyph.Create(title);

        var open = new MenuItem { Header = "Open" };
        open.Click += (_, _) => RestoreFromTray();
        var exit = new MenuItem { Header = "Exit" };
        exit.Click += (_, _) => Application.Current.Shutdown();
        _tray.ContextMenu = new ContextMenu { Items = { open, exit } };

        SyncTray(_settings.Current.MinimizeToTray);
        _settings.Updated += (_, s) => SyncTray(s.MinimizeToTray);
        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Minimized && _settings.Current.MinimizeToTray)
            {
                SyncTray(true);
                Hide();
            }
        };
        Closing += (_, _) =>
        {
            try
            {
                if (_tray.IsRegistered)
                {
                    _ = _tray.Unregister();
                }
            }
            catch
            {
                // Best effort on shutdown.
            }
        };
    }

    private void SyncTray(bool enabled)
    {
        try
        {
            if (enabled && !_tray.IsRegistered)
            {
                _ = _tray.Register();
            }
            else if (!enabled && _tray.IsRegistered)
            {
                _ = _tray.Unregister();
            }
        }
        catch
        {
            // Tray is optional chrome; never fail the window for it.
        }
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        _ = Activate();
    }

    private void OnFirstLoaded()
    {
        ViewModel.ApplyResponsiveWidth(ActualWidth);
        if (_settings.Current.Theme == Services.ThemeMode.System)
        {
            SystemThemeWatcher.Watch(this);
        }

        _ = RootNavigation.Navigate(typeof(HomePage));
    }
}
