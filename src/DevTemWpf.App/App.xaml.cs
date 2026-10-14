using System.IO;
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

            builder.Services.Configure<AppOptions>(
                builder.Configuration.GetSection(AppOptions.SectionName));
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<MainWindow>();

            _host = builder.Build();
            await _host.StartAsync().ConfigureAwait(true);

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
