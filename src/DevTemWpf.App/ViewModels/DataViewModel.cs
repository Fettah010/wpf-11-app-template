using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Data gallery page: editable grid, tree, and time input (1.3.0).</summary>
public sealed partial class DataViewModel : ObservableObject, INavigationAware
{
    public DataViewModel(DataGridViewModel grid)
    {
        Grid = grid;
    }

    public DataGridViewModel Grid { get; }

    // No reload on navigation: the grid loads eagerly in its constructor and
    // refreshes explicitly via Refresh/Retry. A reload here would race
    // in-flight filter and edit gestures every time the page opens.
    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
