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
    private readonly IBackdropService _backdrop;

    public MainWindow(
        MainViewModel viewModel,
        IOptions<AppOptions> options,
        INavigationViewPageProvider pageProvider,
        ISettingsService settings,
        IBackdropService backdrop)
    {
        _settings = settings;
        _backdrop = backdrop;

        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;
        Title = options.Value.Title;
        Width = options.Value.Width;
        Height = options.Value.Height;

        RootNavigation.SetPageProviderService(pageProvider);

        SizeChanged += (_, e) => ViewModel.ApplyResponsiveWidth(e.NewSize.Width);
        Loaded += (_, _) => OnFirstLoaded();
        _settings.Updated += (_, _) => ApplyChrome();
    }

    public MainViewModel ViewModel { get; }

    private void OnFirstLoaded()
    {
        ViewModel.ApplyResponsiveWidth(ActualWidth);
        ApplyChrome();
        if (_settings.Current.Theme == Services.ThemeMode.System)
        {
            SystemThemeWatcher.Watch(this);
        }

        _ = RootNavigation.Navigate(typeof(HomePage));
    }

    private void ApplyChrome()
    {
        // Extend-first: FluentWindow throws if a backdrop is applied without it.
        ExtendsContentIntoTitleBar = true;
        WindowBackdropType = _backdrop.Current(_settings.Current.BackdropEnabled);
    }
}
