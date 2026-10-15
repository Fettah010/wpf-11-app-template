namespace DevTemWpf.Unit;

using DevTemWpf.App.ViewModels;

/// <summary>Time editor behavior: valid sets, range validation, Now.</summary>
public sealed class TimeEditorViewModelTests
{
    [Fact]
    public void Defaults_AreNoon_NoErrors()
    {
        var vm = new TimeEditorViewModel();

        Assert.Equal(new TimeSpan(12, 0, 0), vm.SelectedTime);
        Assert.False(vm.HasErrors);
        Assert.Null(vm.TimeError);
        Assert.Contains("12:00", vm.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidEdit_UpdatesSelectedTime()
    {
        var vm = new TimeEditorViewModel
        {
            Hour = 14,
            Minute = 30,
        };

        Assert.False(vm.HasErrors);
        Assert.Equal(new TimeSpan(14, 30, 0), vm.SelectedTime);
    }

    [Fact]
    public void OutOfRange_SurfacesTextError()
    {
        var vm = new TimeEditorViewModel
        {
            Hour = 99,
        };

        Assert.True(vm.HasErrors);
        Assert.Contains("0-23", vm.TimeError, StringComparison.Ordinal);
        // Invalid edits never move the committed time.
        Assert.Equal(new TimeSpan(12, 0, 0), vm.SelectedTime);
    }

    [Fact]
    public void SetNow_FollowsClock()
    {
        var vm = new TimeEditorViewModel();
        var before = DateTime.Now;

        vm.SetNowCommand.Execute(null);

        Assert.False(vm.HasErrors);
        Assert.True(vm.SelectedTime >= before.TimeOfDay - TimeSpan.FromMinutes(1));
        Assert.True(vm.SelectedTime <= DateTime.Now.TimeOfDay + TimeSpan.FromMinutes(1));
    }
}
