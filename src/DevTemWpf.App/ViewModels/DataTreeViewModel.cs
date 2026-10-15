using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Resources;
using DevTemWpf.App.Services;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Sample tree: selection, expand/collapse-all, and expansion persistence.
/// Expanded node IDs survive restarts via <see cref="ShellSettings"/>.
/// </summary>
public sealed partial class DataTreeViewModel : ObservableObject
{
    private readonly ISettingsService _settings;

    public DataTreeViewModel(ISettingsService settings)
    {
        _settings = settings;
        Nodes = new ObservableCollection<DataTreeNode>(BuildTree());
    }

    public ObservableCollection<DataTreeNode> Nodes { get; }

    [ObservableProperty]
    private DataTreeNode? selectedNode;

    public string SelectedDetail => SelectedNode is null
        ? Strings.Data_TreeEmptyDetail
        : string.Format(Strings.Data_TreeDetailFormat, SelectedNode.Label, SelectedNode.Children.Count);

    partial void OnSelectedNodeChanged(DataTreeNode? value) => OnPropertyChanged(nameof(SelectedDetail));

    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var node in Walk(Nodes))
        {
            node.IsExpanded = true;
        }
    }

    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var node in Walk(Nodes))
        {
            node.IsExpanded = false;
        }

        SelectedNode = null;
    }

    /// <summary>Re-applies persisted expansion after navigation or restart.</summary>
    public async Task LoadExpandedAsync(CancellationToken cancellationToken = default)
    {
        await _settings.LoadAsync(cancellationToken).ConfigureAwait(false);
        var expanded = _settings.Current.ExpandedDataNodes?.ToHashSet() ?? [];
        foreach (var node in Walk(Nodes))
        {
            node.IsExpanded = expanded.Contains(node.Id);
        }
    }

    /// <summary>Persists expansion (call when leaving the page).</summary>
    public async Task SaveExpandedAsync(CancellationToken cancellationToken = default)
    {
        var ids = Walk(Nodes).Where(n => n.IsExpanded).Select(n => n.Id).ToArray();
        await _settings.UpdateAsync(s => s with { ExpandedDataNodes = ids }, cancellationToken).ConfigureAwait(false);
    }

    public static IEnumerable<DataTreeNode> Walk(IEnumerable<DataTreeNode> roots)
    {
        foreach (var node in roots)
        {
            yield return node;
            foreach (var child in Walk(node.Children))
            {
                yield return child;
            }
        }
    }

    public static List<DataTreeNode> BuildTree()
    {
        var engineering = new DataTreeNode("eng", "Engineering");
        var backend = new DataTreeNode("eng.be", "Backend");
        backend.Children.Add(new DataTreeNode("eng.be.alice", "Alice"));
        backend.Children.Add(new DataTreeNode("eng.be.bob", "Bob"));
        backend.Children.Add(new DataTreeNode("eng.be.cara", "Cara"));
        var frontend = new DataTreeNode("eng.fe", "Frontend");
        frontend.Children.Add(new DataTreeNode("eng.fe.dan", "Dan"));
        frontend.Children.Add(new DataTreeNode("eng.fe.erin", "Erin"));
        var mobile = new DataTreeNode("eng.mo", "Mobile");
        mobile.Children.Add(new DataTreeNode("eng.mo.finn", "Finn"));
        engineering.Children.Add(backend);
        engineering.Children.Add(frontend);
        engineering.Children.Add(mobile);

        var design = new DataTreeNode("des", "Design");
        var ux = new DataTreeNode("des.ux", "UX Research");
        ux.Children.Add(new DataTreeNode("des.ux.gail", "Gail"));
        ux.Children.Add(new DataTreeNode("des.ux.hank", "Hank"));
        var visual = new DataTreeNode("des.vi", "Visual");
        visual.Children.Add(new DataTreeNode("des.vi.ivy", "Ivy"));
        design.Children.Add(ux);
        design.Children.Add(visual);

        var operations = new DataTreeNode("ops", "Operations");
        var support = new DataTreeNode("ops.su", "Support");
        support.Children.Add(new DataTreeNode("ops.su.l1", "Level 1"));
        support.Children.Add(new DataTreeNode("ops.su.l2", "Level 2"));
        var sre = new DataTreeNode("ops.sre", "Reliability");
        sre.Children.Add(new DataTreeNode("ops.sre.jay", "Jay"));
        sre.Children.Add(new DataTreeNode("ops.sre.kim", "Kim"));
        operations.Children.Add(support);
        operations.Children.Add(sre);

        return [engineering, design, operations];
    }

    /// <summary>Wide deterministic tree for scale tests (no UI binding).</summary>
    public static List<DataTreeNode> BuildLargeTree(int branches, int leavesPerBranch)
    {
        var roots = new List<DataTreeNode>(branches);
        for (var b = 0; b < branches; b++)
        {
            var branch = new DataTreeNode($"bulk.{b}", $"Branch {b}");
            for (var l = 0; l < leavesPerBranch; l++)
            {
                branch.Children.Add(new DataTreeNode($"bulk.{b}.{l}", $"Leaf {b}.{l}"));
            }

            roots.Add(branch);
        }

        return roots;
    }
}
