using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            _host = builder.Build();
            await _host.StartAsync().ConfigureAwait(true);

            // Settings → theme before the window exists (ADR-0006).
            var settings = _host.Services.GetRequiredService<DevTemWpf.App.Services.ISettingsService>();
            await settings.LoadAsync().ConfigureAwait(true);
            var theme = _host.Services.GetRequiredService<DevTemWpf.App.Services.IThemeService>();
            await theme.ApplyCurrentAsync().ConfigureAwait(true);

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
