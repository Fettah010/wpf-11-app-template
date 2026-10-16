using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Data gallery page (view wiring only).</summary>
public partial class DataPage : Page, INavigableView<DataViewModel>
{
    public DataPage(DataViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public DataViewModel ViewModel { get; }

    /// <summary>Category index: scrolls the section into view and focuses its heading.</summary>
    private void OnIndexClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string targetName })
        {
            return;
        }

        if (FindName(targetName) is not FrameworkElement target)
        {
            return;
        }

        var offset = target.TransformToVisual(DataScroller).Transform(new Point(0, 0));
        DataScroller.ScrollToVerticalOffset(DataScroller.VerticalOffset + offset.Y);
        target.Focus();
    }

    /// <summary>TreeView.SelectedItem is read-only: sync selection into the ViewModel here.</summary>
    private void OnTreeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        ViewModel.Tree.SelectedNode = e.NewValue as DataTreeNode;
    }
}
