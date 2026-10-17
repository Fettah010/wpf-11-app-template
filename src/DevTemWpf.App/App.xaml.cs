using System.Windows;
using DevTemWpf.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wpf.Ui.Appearance;

namespace DevTemWpf.App;

/// <summary>
/// Composition root. Window lifecycle and host wiring only;
/// no business logic in code-behind (protocol rule 7).
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Culture FIRST (1.3.0): every thread created afterwards (host,
            // threadpool, page construction) must inherit it — applying it
            // after StartAsync left page construction on en-US while VM
            // lookups already saw German (see 1.3.0 report). No host needed.
            var earlySettings = new DevTemWpf.App.Services.SettingsService();
            await earlySettings.LoadAsync().ConfigureAwait(true);
            DevTemWpf.App.Services.CultureService.ApplyCulture(earlySettings.Current.Culture);
            DevTemWpf.App.Resources.Strings.OverrideCulture =
                DevTemWpf.App.Services.CultureService.CultureOrNull(earlySettings.Current.Culture);

            var builder = Host.CreateApplicationBuilder(e.Args);
            builder.ConfigureForApp();
            //#if (IncludeLogging)
            builder.AddAppLogging();
            //#endif

            //#if (IncludeUpdates)
            // Velopack install/update hooks (safe when not installed).
            Velopack.VelopackApp.Build().Run();
            //#endif

            _host = builder.Build();
            await _host.StartAsync().ConfigureAwait(true);

            //#if (IncludeCrash)
            // Local-first crash capture (ADR-0017/D-1): redacted per-user file,
            // no vendor/telemetry. No-op when Crash:Enabled is false.
            _host.Services.GetRequiredService<DevTemWpf.App.Services.ICrashReporter>().Install();
            //#endif

            //#if (IncludeAuth)
            // Silent, cache-only session restore (ADR-0017/D-2): no interaction
            // and no network unless Auth is configured.
            await _host.Services.GetRequiredService<DevTemWpf.App.Services.IAuthService>()
                .TryRestoreAsync().ConfigureAwait(true);
            //#endif

            var startupLog = _host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
                .CreateLogger("Startup");
            startupLog.LogInformation(
                "Starting DevTemWpf {Version} on {OS}",
                GetType().Assembly.GetName().Version,
                Environment.OSVersion.VersionString);

            // Settings → theme before the window exists (ADR-0006).
            var settings = _host.Services.GetRequiredService<DevTemWpf.App.Services.ISettingsService>();
            if (await settings.LoadAsync().ConfigureAwait(true))
            {
                // Persist (creates the file on first run, repairs corrupt files).
                await settings.UpdateAsync(static s => s).ConfigureAwait(true);
            }

            // Re-assert the culture on the dispatcher thread: continuations
            // may have resumed on a pool thread whose culture never saw the
            // early application above.
            var cultureOk = false;
            Dispatcher.Invoke(() => cultureOk = DevTemWpf.App.Services.CultureService.ApplyCulture(settings.Current.Culture));
            DevTemWpf.App.Resources.Strings.OverrideCulture =
                DevTemWpf.App.Services.CultureService.CultureOrNull(settings.Current.Culture);
            if (!cultureOk)
            {
                startupLog.LogWarning("Unknown culture '{Culture}'; using system culture.", settings.Current.Culture);
            }
            var theme = _host.Services.GetRequiredService<DevTemWpf.App.Services.IThemeService>();
            await theme.ApplyCurrentAsync().ConfigureAwait(true);

            // OS theme switches reset accents to the OS color inside WPF-UI;
            // re-apply only the accent override (accent updates raise no
            // Changed event, so this cannot recurse).
            var host = _host;
            ApplicationThemeManager.Changed += (_, _) =>
            {
                var current = host.Services.GetRequiredService<DevTemWpf.App.Services.ISettingsService>().Current;
                host.Services.GetRequiredService<DevTemWpf.App.Services.IThemeApplier>()
                    .ApplyAccentOverride(current.AccentOverride, ApplicationThemeManager.GetAppTheme());
            };

            ThemeBootstrap.EnsureLoaded(this);

            var window = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Application failed to start:{Environment.NewLine}{ex.Message}",
                "DevTemWpf — Startup failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);
            _host.Dispose();
            _host = null;
        }

        base.OnExit(e);
    }
}
