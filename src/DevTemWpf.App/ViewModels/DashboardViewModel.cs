using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Dashboard: metrics, chart, sortable/filterable table, detail, stepper.</summary>
public sealed partial class DashboardViewModel : ObservableObject, INavigationAware
{
    private static readonly string[] StepTitles = ["Connect", "Choose", "Review"];
    private static readonly string[] StepBodies =
    [
        "Step 1 of 3 — everything on this page is deterministic sample data.",
        "Step 2 of 3 — filter or sort the activity table, then pick a row.",
        "Step 3 of 3 — review the detail card. Done (nothing is provisioned).",
    ];

    private readonly List<ActivityItem> _all;
    private readonly FavoritesStore _favorites;
    private string _sortColumn = nameof(ActivityItem.Timestamp);
    private ListSortDirection _sortDirection = ListSortDirection.Descending;

    public DashboardViewModel(SampleDataService data, FavoritesStore favorites)
    {
        _favorites = favorites;
        Metrics = data.GetMetrics();
        _all = data.GetActivities().ToList();
        Activities = CollectionViewSource.GetDefaultView(_all);
        Activities.Filter = Matches;
        ApplySort();

        Series =
        [
            new LineSeries<double>
            {
                Values = data.GetWeeklyValues(),
                Fill = null,
                GeometrySize = 8,
                Name = "Completed tasks",
            },
        ];
        XAxes = [new Axis { Labels = data.GetWeekdayLabels().ToList() }];
        UpdateStep();
    }

    public IReadOnlyList<MetricItem> Metrics { get; }

    public ICollectionView Activities { get; }

    public ISeries[] Series { get; }

    public Axis[] XAxes { get; }

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ActivityItem? selectedActivity;

    [ObservableProperty]
    private string favoriteGlyph = "☆ Mark favorite";

    [ObservableProperty]
    private string sortStatusText = string.Empty;

    [ObservableProperty]
    private string? noResultsMessage;

    [ObservableProperty]
    private string stepTitle = string.Empty;

    [ObservableProperty]
    private string stepBody = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepProgress))]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousStepCommand))]
    private int stepIndex;

    public bool HasResults => !Activities.IsEmpty;

    public int StepProgress => (StepIndex + 1) * 100 / StepTitles.Length;

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    partial void OnSearchTextChanged(string value)
    {
        Activities.Refresh();
        UpdateResultFlags();
    }

    [RelayCommand]
    private void ClearSelection() => SelectedActivity = null;

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (SelectedActivity is null)
        {
            return;
        }

        _favorites.ToggleFavorite(SelectedActivity.Id);
        UpdateFavoriteGlyph();
    }

    partial void OnSelectedActivityChanged(ActivityItem? value) => UpdateFavoriteGlyph();

    private void UpdateFavoriteGlyph()
    {
        FavoriteGlyph = SelectedActivity is not null && _favorites.IsFavorite(SelectedActivity.Id)
            ? "★ Favorited — tap to remove"
            : "☆ Mark favorite";
    }

    [RelayCommand]
    private void SortBy(string column)
    {
        var property = column switch
        {
            nameof(ActivityItem.Timestamp) => nameof(ActivityItem.Timestamp),
            nameof(ActivityItem.Name) => nameof(ActivityItem.Name),
            nameof(ActivityItem.Category) => nameof(ActivityItem.Category),
            nameof(ActivityItem.Status) => nameof(ActivityItem.Status),
            nameof(ActivityItem.Value) => nameof(ActivityItem.Value),
            _ => nameof(ActivityItem.Timestamp),
        };

        _sortDirection = property == _sortColumn && _sortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : property == _sortColumn ? _sortDirection : ListSortDirection.Ascending;
        _sortColumn = property;
        ApplySort();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextStep()
    {
        StepIndex = Math.Min(StepIndex + 1, StepTitles.Length - 1);
        UpdateStep();
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousStep()
    {
        StepIndex = Math.Max(StepIndex - 1, 0);
        UpdateStep();
    }

    private bool CanGoNext() => StepIndex < StepTitles.Length - 1;

    private bool CanGoPrevious() => StepIndex > 0;

    private void ApplySort()
    {
        Activities.SortDescriptions.Clear();
        Activities.SortDescriptions.Add(new SortDescription(_sortColumn, _sortDirection));
        SortStatusText = $"Sorted by {_sortColumn} {(_sortDirection == ListSortDirection.Ascending ? "↑" : "↓")} · {_all.Count} rows";
        UpdateResultFlags();
    }

    private void UpdateResultFlags()
    {
        OnPropertyChanged(nameof(HasResults));
        NoResultsMessage = HasResults ? null : $"No activities match \"{SearchText}\".";
    }

    private bool Matches(object item)
    {
        if (item is not ActivityItem activity || string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return activity.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || activity.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || activity.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateStep()
    {
        StepTitle = StepTitles[StepIndex];
        StepBody = StepBodies[StepIndex];
    }
}
