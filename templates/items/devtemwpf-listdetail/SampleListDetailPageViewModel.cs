using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Models;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>SampleListDetailPage ViewModel.</summary>
public sealed partial class SampleListDetailPageViewModel : ObservableObject, INavigationAware
{
    private readonly List<SampleListItem> _allItems =
    [
        new(1, "Alpha Task", "Primary initialization step for system tasks.", "Setup", DateTime.UtcNow.AddDays(-2)),
        new(2, "Beta Configuration", "Secondary network configuration checks.", "Network", DateTime.UtcNow.AddDays(-1)),
        new(3, "Gamma Diagnostics", "Full system metric sweep and diagnostics report.", "Diagnostics", DateTime.UtcNow)
    ];

    [ObservableProperty]
    private ObservableCollection<SampleListItem> _items = [];

    [ObservableProperty]
    private SampleListItem? _selectedItem;

    [ObservableProperty]
    private string _filterText = string.Empty;

    public SampleListDetailPageViewModel()
    {
        RefreshFilter();
    }

    partial void OnFilterTextChanged(string value) => RefreshFilter();

    private void RefreshFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allItems
            : _allItems.Where(i => i.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                                  i.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();

        Items = new ObservableCollection<SampleListItem>(filtered);
        if (SelectedItem == null || !Items.Contains(SelectedItem))
        {
            SelectedItem = Items.FirstOrDefault();
        }
    }

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
