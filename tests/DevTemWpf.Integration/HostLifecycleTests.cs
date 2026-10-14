using DevTemWpf.App;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using DevTemWpf.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Wpf.Ui.Abstractions;

namespace DevTemWpf.Integration;

public sealed class HostLifecycleTests
{
    [WpfFact]
    public async Task Host_Builds_ResolvesServices_And_StopsCleanly()
    {
        using var host = Host.CreateApplicationBuilder()
            .ConfigureForApp()
            .Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            var vm = host.Services.GetRequiredService<MainViewModel>();
            var options = host.Services.GetRequiredService<IOptions<AppOptions>>();

            Assert.NotNull(vm);
            Assert.False(string.IsNullOrWhiteSpace(options.Value.Title));

            // 0.2.0 shell graph resolves (controls are never created here — MTA-safe).
            Assert.NotNull(host.Services.GetRequiredService<ISettingsService>());
            Assert.NotNull(host.Services.GetRequiredService<IThemeService>());
            Assert.NotNull(host.Services.GetRequiredService<IBackdropService>());
            Assert.NotNull(host.Services.GetRequiredService<INavigationViewPageProvider>());
            Assert.NotNull(host.Services.GetRequiredService<HomeViewModel>());
            Assert.NotNull(host.Services.GetRequiredService<SettingsViewModel>());
            Assert.NotNull(host.Services.GetRequiredService<HomePage>());
            Assert.NotNull(host.Services.GetRequiredService<SettingsPage>());
        }
        finally
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }
}
