using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevTemWpf.App.Models;

/// <summary>Hierarchical tree node model (supports expand state + children).</summary>
public sealed partial class SampleTreeNode : ObservableObject
{
    public SampleTreeNode(
        int id,
        string title,
        string description,
        int level,
        IEnumerable<SampleTreeNode>? children = null)
    {
        Id = id;
        Title = title;
        Description = description;
        Level = level;
        Children = new ObservableCollection<SampleTreeNode>(children ?? []);
    }

    public int Id { get; }
    public string Title { get; }
    public string Description { get; }
    public int Level { get; }
    public ObservableCollection<SampleTreeNode> Children { get; }

    [ObservableProperty]
    private bool _isExpanded;
}
