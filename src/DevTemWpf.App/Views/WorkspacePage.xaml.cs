using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Document-workspace page for the Tabbed shell (view wiring only).</summary>
public partial class WorkspacePage : Page, INavigableView<WorkspaceViewModel>
{
    /// <summary>
    /// Private drag format: route keys never leave the workspace, so no
    /// cross-process/cross-window drop is possible by construction.
    /// </summary>
    internal const string TabDragFormat = "DevTemWpf.DocumentTab";

    private Point _dragStart;

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

    /// <summary>Records a potential drag origin on tab headers.</summary>
    private void OnTabDragStart(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(DocumentTabs);
    }

    /// <summary>
    /// Starts a move-drag once the pointer leaves the drag threshold on a
    /// tab header. Esc cancels natively; the keyboard alternative is the
    /// detail-pane Move buttons.
    /// </summary>
    private void OnTabDragMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var position = e.GetPosition(DocumentTabs);
        if (Math.Abs(position.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(position.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (FindTabDocument(position) is not { } document)
        {
            return;
        }

        _ = DragDrop.DoDragDrop(
            DocumentTabs,
            new DataObject(TabDragFormat, document.RouteKey),
            DragDropEffects.Move);
    }

    private void OnTabDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(TabDragFormat)
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>Drops a dragged tab at the target header's index.</summary>
    private void OnTabDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(TabDragFormat) is not string key)
        {
            return;
        }

        var target = DocumentTabs.ItemContainerGenerator.IndexFromContainer(
            FindTabItem(e.GetPosition(DocumentTabs)));
        var document = ViewModel.Documents.FirstOrDefault(d => d.RouteKey == key);
        if (target < 0 || document is null)
        {
            return;
        }

        _ = ViewModel.MoveDocumentToAsync(document, target);
    }

    private WorkspaceViewModel.Document? FindTabDocument(Point position)
    {
        var item = FindTabItem(position);
        return item is null
            ? null
            : DocumentTabs.ItemContainerGenerator.ItemFromContainer(item) as WorkspaceViewModel.Document;
    }

    private DependencyObject? FindTabItem(Point position)
    {
        // Hit-testing runs on the UI thread; all callers are input handlers.
        var hit = VisualTreeHelper.HitTest(DocumentTabs, position)?.VisualHit;
        while (hit is not null && hit is not TabItem)
        {
            hit = VisualTreeHelper.GetParent(hit);
        }

        return hit;
    }
}
