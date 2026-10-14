using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using DevTemWpf.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui.DependencyInjection;

namespace DevTemWpf.App;

/// <summary>
/// Shared composition root used by <see cref="App"/> and integration tests,
/// so tests exercise the real registrations (ADR-0002).
/// </summary>
public static class AppComposition
{
    public static HostApplicationBuilder ConfigureForApp(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<AppOptions>(
            builder.Configuration.GetSection(AppOptions.SectionName));

        // Shell services (ADR-0006)
        builder.Services.AddSingleton<ISettingsService, SettingsService>();

        // Dashboard sample data (ADR-0010; excluded from clean scaffolds)
        builder.Services.AddSingleton<SampleDataService>();        builder.Services.AddSingleton<IBackdropService, BackdropService>();
        builder.Services.AddSingleton<IThemeApplier, WpfUiThemeApplier>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IMotionService, MotionService>();

        // WPF-UI navigation (ADR-0005): v4 resolves pages through
        // INavigationViewPageProvider; no IPageService/ PageService types ship
        // in 4.3.0, and VM-initiated navigation is not needed in 0.2.0
        // (item clicks + back button are automatic, initial nav is code-behind).
        _ = builder.Services.AddNavigationViewPageProvider();

        // Feedback services (ADR-0008)
        builder.Services.AddSingleton<Wpf.Ui.ISnackbarService, Wpf.Ui.SnackbarService>();
        builder.Services.AddSingleton<Wpf.Ui.IContentDialogService, Wpf.Ui.ContentDialogService>();

        // Shell + pages
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<MainWindow>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<TokensViewModel>();
        builder.Services.AddTransient<ControlsViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<EffectsViewModel>();
        builder.Services.AddTransient<FormViewModel>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<TokensPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<EffectsPage>();
        builder.Services.AddTransient<ControlsPage>();
        return builder;
    }
}
