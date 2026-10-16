namespace DevTemWpf.Unit;

using DevTemWpf.App.ViewModels;

/// <summary>DataGrid sample behavior: filter, selection, validation, states, scale.</summary>
public sealed class DataGridViewModelTests
{
    [Fact]
    public void Constructor_LoadsFirstPageSynchronously()
    {
        var vm = new DataGridViewModel();

        Assert.Equal(DataLoadState.Loaded, vm.LoadState);
        Assert.Equal(200, vm.Rows.Count);
        Assert.False(vm.IsLoading);
        Assert.False(vm.ShowEmpty);
    }

    [Fact]
    public async Task Refresh_LoadsDeterministicRows()
    {
        var vm = new DataGridViewModel();

        await vm.RefreshAsync(50);

        Assert.Equal(DataLoadState.Loaded, vm.LoadState);
        Assert.Equal(50, vm.Rows.Count);
        Assert.Equal(vm.Rows.Count, vm.View.Cast<object>().Count());
        Assert.Contains("50 of 50 rows", vm.CountText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_NarrowsRows_AndEmptyStateShows()
    {
        var vm = new DataGridViewModel();
        await vm.RefreshAsync(50);

        vm.SearchText = "Task 00001";

        Assert.True(vm.View.Cast<object>().Count() < 50);
        vm.SearchText = "zzz-no-match";
        Assert.True(vm.View.IsEmpty);
        Assert.True(vm.ShowEmpty);

        vm.SearchText = string.Empty;
        Assert.Equal(50, vm.View.Cast<object>().Count());
        Assert.False(vm.ShowEmpty);
    }

    [Fact]
    public async Task SimulatedError_MapsToError_WithRetryRecovery()
    {
        var vm = new DataGridViewModel
        {
            SimulateError = true,
        };

        await vm.RefreshAsync(50);

        Assert.Equal(DataLoadState.Error, vm.LoadState);
        Assert.True(vm.HasError);
        Assert.Contains("Simulated load failure", vm.StatusText, StringComparison.Ordinal);

        vm.SimulateError = false;
        await vm.RetryCommand.ExecuteAsync(null);

        Assert.Equal(DataLoadState.Loaded, vm.LoadState);
        Assert.Equal(50, vm.Rows.Count);
    }

    [Fact]
    public void RowValidation_RejectsBadValues()
    {
        var row = new DataGridRow("Ok name", "Open", 5);

        row.Name = string.Empty;
        row.Value = -1;

        Assert.True(row.HasErrors);
        Assert.NotEmpty(row.GetErrors(nameof(DataGridRow.Name)));
        Assert.NotEmpty(row.GetErrors(nameof(DataGridRow.Value)));
    }

    [Fact]
    public void RowValidation_AcceptsGoodValues()
    {
        var row = new DataGridRow("Task 1", "Done", 100);

        Assert.False(row.HasErrors);
    }

    [Fact]
    public void Filter_RaisesCollectionChanged()
    {
        var vm = new DataGridViewModel();
        var events = 0;
        vm.View.CollectionChanged += (_, _) => events++;

        vm.SearchText = "zzz-no-match";

        Assert.True(events > 0, "View.Refresh() did not raise CollectionChanged.");
    }

    [Fact]
    public async Task TenThousandRows_FilterStaysCorrect()
    {
        var vm = new DataGridViewModel();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await vm.RefreshAsync(10000);
        sw.Stop();

        Assert.Equal(10000, vm.Rows.Count);
        vm.SearchText = "Blocked";
        var shown = vm.View.Cast<object>().Count();
        Assert.True(shown > 0 && shown < 10000);
        // Correctness-first bound: catches pathological hangs, never flakes on CI.
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(30), $"10k load took {sw.Elapsed}");
    }
}
