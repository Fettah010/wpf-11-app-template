using System.Windows;
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

    public MainWindow(
        MainViewModel viewModel,
        IOptions<AppOptions> options,
        INavigationViewPageProvider pageProvider,
        Wpf.Ui.ISnackbarService snackbar,
        Wpf.Ui.IContentDialogService dialogs,
        ISettingsService settings,
        IBackdropService backdrop)
    {
        _settings = settings;

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

        SizeChanged += (_, e) => ViewModel.ApplyResponsiveWidth(e.NewSize.Width);
        Loaded += (_, _) => OnFirstLoaded();
        _settings.Updated += (_, s) => DesignTokenBootstrap.ApplyDensity(s.Density);
    }

    public MainViewModel ViewModel { get; }

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
