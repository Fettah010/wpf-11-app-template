using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
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
    private readonly WorkspaceViewModel _workspace;
    private readonly MotionScope _motionScope;
    private Services.ThemeMode _lastTheme;
    private IInputElement? _paletteInvoker;
    //#if (IncludeTray)
    private readonly Wpf.Ui.Tray.INotifyIconService _tray;
    //#endif

    public MainWindow(
        MainViewModel viewModel,
        WorkspaceViewModel workspace,
        IOptions<AppOptions> options,
        INavigationViewPageProvider pageProvider,
        Wpf.Ui.ISnackbarService snackbar,
        Wpf.Ui.IContentDialogService dialogs,
        //#if (IncludeTray)
        Wpf.Ui.Tray.INotifyIconService tray,
        //#endif
        ISettingsService settings,
        IBackdropService backdrop,
        MotionScope motionScope)
    {
        _settings = settings;
        _workspace = workspace;
        _motionScope = motionScope;
        _lastTheme = settings.Current.Theme;
        //#if (IncludeTray)
        _tray = tray;
        //#endif

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
        RootNavigation.Navigating += OnNavNavigating;
        _workspace.PropertyChanged += OnWorkspaceSelectionChanged;
        //#if (IncludeTray)
        SetupTray(options.Value.Title);
        //#endif

        SizeChanged += (_, e) => ViewModel.ApplyResponsiveWidth(e.NewSize.Width);
        Loaded += (_, _) => OnFirstLoaded();
        _settings.Updated += (_, s) =>
        {
            DesignTokenBootstrap.ApplyDensity(s.Density);
            if (s.Theme != _lastTheme)
            {
                _lastTheme = s.Theme;
                _motionScope.StopAll();
            }
        };
        PreviewKeyDown += OnWindowPreviewKeyDown;

        // Motion must not outlive the window's focus or lifetime (ADR-0016).
        Deactivated += (_, _) => _motionScope.StopAll();
        Closing += (_, _) => _motionScope.StopAll();

        Palette = new PaletteViewModel();
        Palette.NavigateRoute = GoToRoute;
    }

    public MainViewModel ViewModel { get; }

    /// <summary>Command palette model (1.4.0); overlay chrome is in XAML above.</summary>
    public PaletteViewModel Palette { get; }

    //#if (IncludeTray)
    /// <summary>Tray: icon exists only while minimize-to-tray is enabled.</summary>
    private void SetupTray(string title)
    {
        _tray.SetParentWindow(this);
        _tray.TooltipText = title;
        _tray.Icon = TrayGlyph.Create(title);

        var open = new MenuItem { Header = DevTemWpf.App.Resources.Strings.Tray_Open };
        open.Click += (_, _) => RestoreFromTray();
        var exit = new MenuItem { Header = DevTemWpf.App.Resources.Strings.Tray_Exit };
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
    //#endif

    private void OnFirstLoaded()
    {
        ViewModel.ApplyResponsiveWidth(ActualWidth);
        if (_settings.Current.Theme == Services.ThemeMode.System)
        {
            SystemThemeWatcher.Watch(this);
        }

        // Tabbed shell hosts the document workspace instead of a start page.
        _ = RootNavigation.Navigate(_settings.Current.Shell == ShellMode.Tabbed
            ? typeof(WorkspacePage)
            : typeof(HomePage));
    }

    /// <summary>Global Ctrl+K opens the palette from anywhere in the shell.</summary>
    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.K && Keyboard.Modifiers == ModifierKeys.Control)
        {
            OpenPalette();
            e.Handled = true;
        }
    }

    private void OnPaletteButtonClick(object sender, RoutedEventArgs e) => OpenPalette();

    /// <summary>Opens the palette, focusing the filter; remembers the invoker for focus return.</summary>
    private void OpenPalette()
    {
        _paletteInvoker = Keyboard.FocusedElement;
        Palette.Query = string.Empty;
        PaletteOverlay.Visibility = Visibility.Visible;
        _ = PaletteFilter.Focus();
    }

    /// <summary>Closes the palette, returning focus to its invoker.</summary>
    private void ClosePalette()
    {
        PaletteOverlay.Visibility = Visibility.Collapsed;
        if (_paletteInvoker is not null)
        {
            _ = Keyboard.Focus(_paletteInvoker);
            _paletteInvoker = null;
        }
    }

    /// <summary>Executes a palette route in the active shell, then focuses its nav item.</summary>
    private void GoToRoute(string routeKey)
    {
        var route = NavigationRoutes.Find(routeKey);
        if (route is null)
        {
            return;
        }

        if (_settings.Current.Shell == ShellMode.Tabbed)
        {
            _ = _workspace.OpenDocumentAsync(route.Key);
        }
        else
        {
            _ = RootNavigation.Navigate(route.PageType);
        }

        ClosePalette();
        FocusNavItem(routeKey);
    }

    private void FocusNavItem(string routeKey)
    {
        var route = NavigationRoutes.Find(routeKey);
        var item = route is null
            ? null
            : ViewModel.MenuItems.OfType<Wpf.Ui.Controls.NavigationViewItem>()
                .Concat(ViewModel.FooterMenuItems.OfType<Wpf.Ui.Controls.NavigationViewItem>())
                .FirstOrDefault(i => i.TargetPageType == route.PageType);
        if (item is not null)
        {
            _ = item.Focus();
        }
        else
        {
            _ = Focus();
        }
    }

    private void OnPaletteFilterKey(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (Palette.SelectedResult is not null)
                {
                    Palette.ExecuteSelectedCommand.Execute(null);
                }

                e.Handled = true;
                break;
            case Key.Escape:
                ClosePalette();
                e.Handled = true;
                break;
            case Key.Down:
                Palette.MoveSelection(1);
                e.Handled = true;
                break;
            case Key.Up:
                Palette.MoveSelection(-1);
                e.Handled = true;
                break;
        }
    }

    private void OnPaletteResultsKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Palette.ExecuteSelectedCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            ClosePalette();
            e.Handled = true;
        }
    }

    private void OnPaletteResultsDoubleClick(object sender, MouseButtonEventArgs e)
    {
        Palette.ExecuteSelectedCommand.Execute(null);
    }

    private void OnPaletteScrimClick(object sender, MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, PaletteOverlay))
        {
            ClosePalette();
        }
    }

    /// <summary>
    /// Tabbed shell (1.4.0): rail clicks open route-keyed documents in the
    /// workspace instead of frame-navigating. The frame stays on the
    /// workspace page; unknown pages fall back to normal navigation.
    /// </summary>
    private void OnNavNavigating(
        Wpf.Ui.Controls.NavigationView sender,
        Wpf.Ui.Controls.NavigatingCancelEventArgs e)
    {
        if (_settings.Current.Shell != ShellMode.Tabbed)
        {
            return;
        }

        var type = e.Page as Type ?? (e.Page as FrameworkElement)?.GetType();
        var route = type is null
            ? null
            : NavigationRoutes.All.FirstOrDefault(r => r.PageType == type);
        if (route is null)
        {
            return;
        }

        e.Cancel = true;
        _ = _workspace.OpenDocumentAsync(route.Key);
    }

    /// <summary>
    /// Tabbed shell: reflect the selected document in the rail via the
    /// items' active flag (the control's SelectedItem setter is internal;
    /// IsActive is the documented visual-selection flag and setting it
    /// directly performs no navigation).
    /// </summary>
    private void OnWorkspaceSelectionChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(WorkspaceViewModel.SelectedDocument))
        {
            return;
        }

        if (_settings.Current.Shell != ShellMode.Tabbed)
        {
            return;
        }

        var key = _workspace.SelectedDocument?.RouteKey;
        var route = key is null ? null : NavigationRoutes.Find(key);
        if (route is null)
        {
            return;
        }

        foreach (var item in ViewModel.MenuItems.OfType<Wpf.Ui.Controls.NavigationViewItem>()
            .Concat(ViewModel.FooterMenuItems.OfType<Wpf.Ui.Controls.NavigationViewItem>()))
        {
            item.IsActive = item.TargetPageType == route.PageType;
        }
    }
}
