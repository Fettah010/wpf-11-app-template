using CommunityToolkit.Mvvm.ComponentModel;

namespace DevTemWpf.App.ViewModels;

/// <summary>One tree node. IsExpanded/IsSelected bind two-way to the TreeViewItem container.</summary>
public sealed partial class DataTreeNode : ObservableObject
{
    public DataTreeNode(string id, string label)
    {
        Id = id;
        Label = label;
    }

    public string Id { get; }

    public string Label { get; }

    public System.Collections.ObjectModel.ObservableCollection<DataTreeNode> Children { get; } = [];

    [ObservableProperty]
    private bool isExpanded;

    [ObservableProperty]
    private bool isSelected;
}
