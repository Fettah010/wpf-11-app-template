using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;

namespace DevTemWpf.Unit;

public sealed class DashboardViewModelTests
{
    private static DashboardViewModel Create() => new(new SampleDataService());

    [Fact]
    public void Filter_NoMatch_ReportsEmptyState()
    {
        var vm = Create();

        vm.SearchText = "zzz-no-such-activity";

        Assert.False(vm.HasResults);
        Assert.Contains("zzz-no-such-activity", vm.NoResultsMessage);
    }

    [Fact]
    public void Filter_Category_NarrowsToMatches()
    {
        var vm = Create();

        vm.SearchText = "Network";

        var rows = vm.Activities.Cast<ActivityItem>().ToList();
        Assert.NotEmpty(rows);
        Assert.True(rows.Count < 1500);
        Assert.All(rows, a => Assert.Equal("Network", a.Category));
        Assert.True(vm.HasResults);
    }

    [Fact]
    public void Filter_Cleared_RestoresAllRows()
    {
        var vm = Create();
        vm.SearchText = "zzz-no-such-activity";

        vm.SearchText = string.Empty;

        Assert.Equal(1500, vm.Activities.Cast<ActivityItem>().Count());
    }

    [Fact]
    public void Sort_TogglesDirection_OnSameColumn()
    {
        var vm = Create();

        vm.SortByCommand.Execute("Value");
        var ascending = vm.Activities.Cast<ActivityItem>().ToList();
        Assert.True(ascending.First().Value <= ascending.Last().Value);
        Assert.Contains("↑", vm.SortStatusText);

        vm.SortByCommand.Execute("Value");
        var descending = vm.Activities.Cast<ActivityItem>().ToList();
        Assert.True(descending.First().Value >= descending.Last().Value);
        Assert.Contains("↓", vm.SortStatusText);
    }

    [Fact]
    public void Sort_UnknownColumn_FallsBackToTimestamp()
    {
        var vm = Create();

        vm.SortByCommand.Execute("Nope");

        Assert.Contains("Timestamp", vm.SortStatusText);
    }

    [Fact]
    public void Selection_HoldsPickedRow()
    {
        var vm = Create();
        var row = vm.Activities.Cast<ActivityItem>().First();

        vm.SelectedActivity = row;

        Assert.Same(row, vm.SelectedActivity);
        vm.ClearSelectionCommand.Execute(null);
        Assert.Null(vm.SelectedActivity);
    }

    [Fact]
    public void Stepper_WalksForwardAndBack_WithProgress()
    {
        var vm = Create();
        Assert.Equal(0, vm.StepIndex);

        vm.NextStepCommand.Execute(null);
        vm.NextStepCommand.Execute(null);
        Assert.Equal(2, vm.StepIndex);
        Assert.Equal(100, vm.StepProgress);
        Assert.False(vm.NextStepCommand.CanExecute(null));

        vm.PreviousStepCommand.Execute(null);
        Assert.Equal(1, vm.StepIndex);
    }

    [Fact]
    public void Stepper_BoundsHold_AtEnds()
    {
        var vm = Create();

        Assert.False(vm.PreviousStepCommand.CanExecute(null));
        vm.PreviousStepCommand.Execute(null);
        Assert.Equal(0, vm.StepIndex);

        vm.NextStepCommand.Execute(null);
        vm.NextStepCommand.Execute(null);
        vm.NextStepCommand.Execute(null);
        Assert.Equal(2, vm.StepIndex);
    }
}
