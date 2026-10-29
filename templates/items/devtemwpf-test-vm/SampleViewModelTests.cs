using DevTemWpf.App.ViewModels;
using Xunit;

namespace DevTemWpf.Unit;

/// <summary>
/// Behavioral unit tests for SampleViewModel: commands and state,
/// not merely file-existence assertions.
/// </summary>
public sealed class SampleViewModelTests
{
    [Fact]
    public void InitialState_DefaultsToZeroAndEmpty()
    {
        var vm = new SampleViewModel();

        Assert.Equal(0, vm.Count);
        Assert.Equal(string.Empty, vm.Title);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public void IncrementCommand_IncreasesCount()
    {
        var vm = new SampleViewModel();

        vm.IncrementCommand.Execute(null);

        Assert.Equal(1, vm.Count);
    }

    [Fact]
    public void DecrementCommand_DecreasesCount()
    {
        var vm = new SampleViewModel { Count = 3 };

        vm.DecrementCommand.Execute(null);

        Assert.Equal(2, vm.Count);
    }

    [Fact]
    public async Task ResetCommand_SetsCountToZeroAndClearsTitle()
    {
        var vm = new SampleViewModel { Count = 42, Title = "sample" };

        await vm.ResetCommand.ExecuteAsync(null);

        Assert.Equal(0, vm.Count);
        Assert.Equal(string.Empty, vm.Title);
        Assert.False(vm.IsBusy);
    }
}
