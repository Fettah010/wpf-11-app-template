using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Models;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>SampleDataGridPage ViewModel.</summary>
public sealed partial class SampleDataGridPageViewModel : ObservableObject, INavigationAware
{
    private readonly List<SampleDataGridRow> _allRows =
    [
        new(101, "SKU-9821", "High-performance compute node", "Active", 14, DateTime.UtcNow.AddHours(-12)),
        new(102, "SKU-4402", "Redundant power supply unit", "Pending", 8, DateTime.UtcNow.AddDays(-1)),
        new(103, "SKU-1193", "Optic fiber transceiver module", "Active", 45, DateTime.UtcNow.AddDays(-2)),
        new(104, "SKU-7720", "Managed 10GbE network switch", "InReview", 3, DateTime.UtcNow.AddDays(-4))
    ];

    [ObservableProperty]
    private ObservableCollection<SampleDataGridRow> _rows = [];

    [ObservableProperty]
    private SampleDataGridRow? _selectedRow;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    public SampleDataGridPageViewModel()
    {
        RefreshData();
    }

    partial void OnSearchTermChanged(string value) => RefreshData();

    [RelayCommand]
    private void RefreshData()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchTerm)
            ? _allRows
            : _allRows.Where(r => r.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                  r.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                  r.Status.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        Rows = new ObservableCollection<SampleDataGridRow>(filtered);
        if (SelectedRow == null || !Rows.Contains(SelectedRow))
        {
            SelectedRow = Rows.FirstOrDefault();
        }
    }

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
