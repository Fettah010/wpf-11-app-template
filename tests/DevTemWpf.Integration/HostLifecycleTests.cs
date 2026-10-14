using DevTemWpf.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Integration;

public sealed class HostLifecycleTests
{
    [Fact]
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
        }
        finally
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }
}
