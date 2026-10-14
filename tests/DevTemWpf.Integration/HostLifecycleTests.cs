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
        TestResources.Ensure();
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

            // 0.2.0 shell graph resolves. Pages construct here (0.3.0 XAML guard).
            Assert.NotNull(host.Services.GetRequiredService<ISettingsService>());
            Assert.NotNull(host.Services.GetRequiredService<IThemeService>());
            Assert.NotNull(host.Services.GetRequiredService<IBackdropService>());
            Assert.NotNull(host.Services.GetRequiredService<INavigationViewPageProvider>());
            Assert.NotNull(host.Services.GetRequiredService<Wpf.Ui.ISnackbarService>());
            Assert.NotNull(host.Services.GetRequiredService<Wpf.Ui.IContentDialogService>());
            Assert.NotNull(host.Services.GetRequiredService<DevTemWpf.App.Services.IMotionService>());
            Assert.NotNull(host.Services.GetRequiredService<HomeViewModel>());
            Assert.NotNull(host.Services.GetRequiredService<SettingsViewModel>());

            var home = host.Services.GetRequiredService<HomePage>();
            var settings = host.Services.GetRequiredService<SettingsPage>();
            var tokens = host.Services.GetRequiredService<TokensPage>();
            var controls = host.Services.GetRequiredService<ControlsPage>();
            var effects = host.Services.GetRequiredService<EffectsPage>();
            Assert.Same(home, home.DataContext);
            Assert.Same(settings, settings.DataContext);
            Assert.Same(tokens, tokens.DataContext);
            Assert.Same(controls, controls.DataContext);
            Assert.Same(effects, effects.DataContext);
            Assert.NotNull(home.ViewModel);
            Assert.NotNull(settings.ViewModel);
            Assert.NotNull(tokens.ViewModel);
            Assert.NotNull(controls.ViewModel);
            Assert.NotNull(controls.ViewModel.Form);
            Assert.NotNull(effects.ViewModel);
        }
        finally
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }
}
