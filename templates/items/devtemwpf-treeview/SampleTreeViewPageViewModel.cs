using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Models;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>SampleTreeViewPage ViewModel.</summary>
public sealed partial class SampleTreeViewPageViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private ObservableCollection<SampleTreeNode> _nodes = [];

    [ObservableProperty]
    private SampleTreeNode? _selectedNode;

    public SampleTreeViewPageViewModel()
    {
        RefreshNodes();
    }

    /// <summary>Rebuilds the sample hierarchy; replace with a real data source.</summary>
    private void RefreshNodes()
    {
        var roots = new List<SampleTreeNode>
        {
            new(1, "System", "Core operating system components", 0,
            [
                new(11, "Kernel", "Process and memory management", 1),
                new(12, "Drivers", "Device driver layer", 1),
                new(13, "Shell", "User interface and command shell", 1)
            ])
            {
                IsExpanded = true
            },
            new(2, "Devices", "Connected hardware and peripherals", 0,
            [
                new(21, "Display", "Graphics adapters and monitors", 1),
                new(22, "Input", "Keyboard, mouse, and touch", 1),
                new(23, "Storage", "Disk and USB controllers", 1)
            ]),
            new(3, "Network", "Network interfaces and services", 0,
            [
                new(31, "Ethernet", "Wired network adapters", 1),
                new(32, "Wi-Fi", "Wireless network adapters", 1),
                new(33, "VPN", "Virtual private network connections", 1)
            ])
        };

        Nodes = new ObservableCollection<SampleTreeNode>(roots);
        SelectedNode = Nodes.FirstOrDefault();
    }

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
