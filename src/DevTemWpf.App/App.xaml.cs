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
            var builder = Host.CreateApplicationBuilder(e.Args);
            builder.ConfigureForApp();
            //#if (IncludeServices)
            builder.AddAppLogging();

            // Velopack install/update hooks (safe when not installed).
            Velopack.VelopackApp.Build().Run();
            //#endif

            _host = builder.Build();
            await _host.StartAsync().ConfigureAwait(true);

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
            if (!DevTemWpf.App.Services.CultureService.ApplyCulture(settings.Current.Culture))
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
