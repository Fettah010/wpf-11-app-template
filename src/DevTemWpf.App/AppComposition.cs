using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<MainWindow>();
        return builder;
    }
}
