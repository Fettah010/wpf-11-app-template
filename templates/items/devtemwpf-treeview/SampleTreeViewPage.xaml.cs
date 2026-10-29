using System.Windows;
using System.Windows.Controls;
using DevTemWpf.App.Models;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>SampleTreeViewPage code-behind.</summary>
public partial class SampleTreeViewPage : Page, INavigableView<SampleTreeViewPageViewModel>
{
    public SampleTreeViewPage(SampleTreeViewPageViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public SampleTreeViewPageViewModel ViewModel { get; }

    private void OnTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // TreeView.SelectedItem is read-only, so the VM is updated from the event.
        ViewModel.SelectedNode = e.NewValue as SampleTreeNode;
    }
}
