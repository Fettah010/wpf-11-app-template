namespace DevTemWpf.Unit;

using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;

/// <summary>Tree sample behavior: structure, expand/collapse, selection, persistence, scale.</summary>
public sealed class DataTreeViewModelTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    private DataTreeViewModel Create()
    {
        var settings = new SettingsService(_path);
        return new DataTreeViewModel(settings);
    }

    [Fact]
    public void Tree_HasDeterministicStructure()
    {
        var vm = Create();
        var all = DataTreeViewModel.Walk(vm.Nodes).ToList();

        Assert.Equal(3, vm.Nodes.Count);
        Assert.True(all.Count > 20);
        Assert.True(all.Select(n => n.Id).Distinct().Count() == all.Count);
    }

    [Fact]
    public void ExpandCollapseAll_TogglesEveryNode()
    {
        var vm = Create();

        vm.ExpandAllCommand.Execute(null);
        Assert.True(DataTreeViewModel.Walk(vm.Nodes).All(n => n.IsExpanded));

        vm.CollapseAllCommand.Execute(null);
        Assert.True(DataTreeViewModel.Walk(vm.Nodes).All(n => !n.IsExpanded));
    }

    [Fact]
    public void Selection_UpdatesDetail()
    {
        var vm = Create();
        var leaf = DataTreeViewModel.Walk(vm.Nodes).First(n => n.Children.Count == 0);

        vm.SelectedNode = leaf;

        Assert.Contains(leaf.Label, vm.SelectedDetail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Expansion_PersistsAcrossInstances()
    {
        var vm = Create();
        var target = DataTreeViewModel.Walk(vm.Nodes).First(n => n.Id == "eng.be");
        target.IsExpanded = true;
        await vm.SaveExpandedAsync(TestContext.Current.CancellationToken);

        var reloaded = Create();
        await reloaded.LoadExpandedAsync(TestContext.Current.CancellationToken);
        var same = DataTreeViewModel.Walk(reloaded.Nodes).First(n => n.Id == "eng.be");

        Assert.True(same.IsExpanded);
        Assert.False(DataTreeViewModel.Walk(reloaded.Nodes).First(n => n.Id == "des.ux").IsExpanded);
    }

    [Fact]
    public void LargeTree_WalkCountsAllNodes()
    {
        var roots = DataTreeViewModel.BuildLargeTree(branches: 100, leavesPerBranch: 100);

        var count = DataTreeViewModel.Walk(roots).Count();

        Assert.Equal(100 * 101, count);
    }
}
