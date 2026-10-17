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
        //#if (IncludeDashboard)
        builder.Services.AddSingleton<SampleDataService>();
        //#endif

        builder.Services.AddSingleton<IBackdropService, BackdropService>();
        builder.Services.AddSingleton<IThemeApplier, WpfUiThemeApplier>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IMotionService, MotionService>();
        builder.Services.AddSingleton<MotionScope>();

        // WPF-UI navigation (ADR-0005): v4 resolves pages through
        // INavigationViewPageProvider; no IPageService/ PageService types ship
        // in 4.3.0, and VM-initiated navigation is not needed in 0.2.0
        // (item clicks + back button are automatic, initial nav is code-behind).
        _ = builder.Services.AddNavigationViewPageProvider();

        // Feedback services (ADR-0008)
        builder.Services.AddSingleton<Wpf.Ui.ISnackbarService, Wpf.Ui.SnackbarService>();
        builder.Services.AddSingleton<Wpf.Ui.IContentDialogService, Wpf.Ui.ContentDialogService>();

        // Production services (ADR-0011). Each is inert without configuration;
        // the 0.8.0 template drops the matching file set + lines + packages.
        //#if (IncludeServices)
        builder.Services.Configure<HttpOptions>(
            builder.Configuration.GetSection(HttpOptions.SectionName));
        builder.Services.Configure<UpdateOptions>(
            builder.Configuration.GetSection(UpdateOptions.SectionName));
        builder.Services.AddSingleton<FavoritesStore>();
        builder.Services.AddSingleton<IBackupService, BackupService>();
        builder.Services.AddSingleton<IUpdateService, UpdateService>();
        builder.Services.AddSingleton<Wpf.Ui.Tray.INotifyIconService, Wpf.Ui.Tray.NotifyIconService>();
        builder.Services.AddHttpClient<StatusClient>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<HttpOptions>>().Value.BaseUrl;
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            {
                client.BaseAddress = uri;
            }
        });
        //#endif

        // Shell + pages
        builder.Services.AddSingleton<MainViewModel>();
        // Workspace state is a singleton: the Tabbed shell, rail sync, and
        // the workspace page share one document collection (1.4.0). Restore
        // is lazy so other shells pay no startup cost (see EnsureLoaded).
        builder.Services.AddSingleton<WorkspaceViewModel>();
        builder.Services.AddTransient<MainWindow>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<TokensViewModel>();
        builder.Services.AddTransient<ControlsViewModel>();
        builder.Services.AddTransient<DataGridViewModel>();
        builder.Services.AddTransient<DataTreeViewModel>();
        builder.Services.AddTransient<TimeEditorViewModel>();
        builder.Services.AddTransient<DataViewModel>();
        //#if (IncludeDashboard)
        builder.Services.AddTransient<DashboardViewModel>();
        //#endif
        //#if (IncludeDiagnostics)
        builder.Services.AddTransient<DiagnosticsViewModel>();
        //#endif
        builder.Services.AddTransient<EffectsViewModel>();
        builder.Services.AddTransient<FormViewModel>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<TokensPage>();
        //#if (IncludeDashboard)
        builder.Services.AddTransient<DashboardPage>();
        //#endif
        //#if (IncludeDiagnostics)
        builder.Services.AddTransient<DiagnosticsPage>();
        //#endif
        builder.Services.AddTransient<EffectsPage>();
        builder.Services.AddTransient<ControlsPage>();
        builder.Services.AddTransient<DataPage>();
        builder.Services.AddTransient<WorkspacePage>();
        return builder;
    }
}
