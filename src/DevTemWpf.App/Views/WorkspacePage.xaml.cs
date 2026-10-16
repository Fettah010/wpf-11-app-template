using System.Windows;
using System.Windows.Controls;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Document-workspace page for the Tabbed shell (view wiring only).</summary>
public partial class WorkspacePage : Page, INavigableView<WorkspaceViewModel>
{
    public WorkspacePage(WorkspaceViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public WorkspaceViewModel ViewModel { get; }

    /// <summary>Splitter drags update the bound width live; persist once released.</summary>
    private void OnSplitterDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        _ = ViewModel.PersistAsync();
    }
}
