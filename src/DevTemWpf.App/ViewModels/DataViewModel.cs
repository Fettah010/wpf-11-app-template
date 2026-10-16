using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Data gallery page: editable grid, tree, and time input (1.3.0).</summary>
public sealed partial class DataViewModel : ObservableObject, INavigationAware
{
    public DataViewModel(DataGridViewModel grid, DataTreeViewModel tree, TimeEditorViewModel time)
    {
        Grid = grid;
        Tree = tree;
        Time = time;
    }

    public DataGridViewModel Grid { get; }

    public DataTreeViewModel Tree { get; }

    public TimeEditorViewModel Time { get; }

    // No grid reload on navigation (see above); tree expansion is re-applied
    // so a restart or a fresh page instance restores the persisted state.
    public async Task OnNavigatedToAsync()
    {
        await Tree.LoadExpandedAsync().ConfigureAwait(true);
    }

    public Task OnNavigatedFromAsync() => Tree.SaveExpandedAsync();
}
