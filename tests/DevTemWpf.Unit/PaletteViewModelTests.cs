using DevTemWpf.App.ViewModels;

namespace DevTemWpf.Unit;

/// <summary>Palette contract (1.4.0): stable IDs, filtering, enabled states.</summary>
public sealed class PaletteViewModelTests
{
    private static PaletteViewModel Create() => new();

    [Fact]
    public void EmptyQuery_ListsEveryRoute_WithStableIds()
    {
        var vm = Create();

        Assert.NotEmpty(vm.Results);
        Assert.Equal(vm.All.Count, vm.Results.Count);
        Assert.Contains(vm.Results, c => c.Id == "nav.home" && c.RouteKey == "home");
        Assert.Contains(vm.Results, c => c.Id == "nav.settings" && c.RouteKey == "settings");
        Assert.Equal(vm.Results.Select(c => c.Id).Distinct().Count(), vm.Results.Count);
    }

    [Fact]
    public void Filter_IsSubstring_CaseInsensitive()
    {
        var vm = Create();

        vm.Query = "tok";
        Assert.Single(vm.Results);
        Assert.Equal("tokens", vm.Results[0].RouteKey);

        vm.Query = "TOK";
        Assert.Single(vm.Results);

        vm.Query = "zzz-no-match";
        Assert.Empty(vm.Results);
        Assert.Null(vm.SelectedResult);
    }

    [Fact]
    public void Execute_InvokesCallback_ForEnabled_And_IgnoresDisabled()
    {
        var vm = new PaletteViewModel(
        [
            new("nav.a", "a", () => "Alpha"),
            new("nav.b", "b", () => "Beta", IsEnabled: false),
        ]);
        string? navigated = null;
        vm.NavigateRoute = key => navigated = key;

        vm.SelectedResult = vm.Results[1];
        vm.ExecuteSelectedCommand.Execute(null);
        Assert.Null(navigated);

        vm.SelectedResult = vm.Results[0];
        vm.ExecuteSelectedCommand.Execute(null);
        Assert.Equal("a", navigated);
    }

    [Fact]
    public void MoveSelection_Clamps_And_SkipsNothing()
    {
        var vm = Create();
        vm.Query = string.Empty;

        var first = vm.Results[0].RouteKey;
        vm.SelectedResult = null;
        vm.MoveSelection(1);
        Assert.Equal(first, vm.SelectedResult?.RouteKey);

        vm.MoveSelection(-1);
        Assert.Equal(first, vm.SelectedResult?.RouteKey);

        vm.MoveSelection(999);
        Assert.Equal(vm.Results[^1].RouteKey, vm.SelectedResult?.RouteKey);
    }
}
