using DevTemWpf.App;
using Microsoft.Extensions.Options;

namespace DevTemWpf.Unit;

public sealed class MainViewModelTests
{
    private static MainViewModel Create(string title = "TestApp")
        => new(Options.Create(new AppOptions { Title = title }));

    [Fact]
    public void Constructor_UsesConfiguredTitleInStatus()
    {
        var vm = Create("TestApp");

        Assert.Contains("TestApp", vm.StatusText);
    }

    [Fact]
    public async Task GreetCommand_UpdatesStatusWithPressCount()
    {
        var vm = Create("TestApp");

        await vm.GreetCommand.ExecuteAsync(null);

        Assert.Contains("pressed 1x", vm.StatusText);
        Assert.Contains("TestApp", vm.StatusText);
    }
}
