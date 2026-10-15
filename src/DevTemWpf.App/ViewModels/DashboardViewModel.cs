using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
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
    private readonly ISettingsService? _settings;
    private readonly Func<string, Color?> _resolveColor;
    private readonly List<double> _weeklyValues;
    private readonly List<string> _weekdayLabels;
    private string _sortColumn = nameof(ActivityItem.Timestamp);
    private ListSortDirection _sortDirection = ListSortDirection.Descending;

    public DashboardViewModel(
        SampleDataService data,
        FavoritesStore favorites,
        ISettingsService? settings = null,
        Func<string, Color?>? resolveColor = null)
    {
        _favorites = favorites;
        _settings = settings;
        _resolveColor = resolveColor ?? DefaultResolveColor;
        Metrics = data.GetMetrics();
        _all = data.GetActivities().ToList();
        Activities = CollectionViewSource.GetDefaultView(_all);
        Activities.Filter = Matches;
        ApplySort();

        _weeklyValues = data.GetWeeklyValues().ToList();
        _weekdayLabels = data.GetWeekdayLabels().ToList();
        BuildChart();
        if (_settings is not null)
        {
            _settings.Updated += OnSettingsUpdated;
        }

        UpdateStep();
    }

    public IReadOnlyList<MetricItem> Metrics { get; }

    public ICollectionView Activities { get; }

    [ObservableProperty]
    private ISeries[] series = [];

    [ObservableProperty]
    private Axis[] xAxes = [];

    [ObservableProperty]
    private Axis[] yAxes = [];

    [ObservableProperty]
    private string chartSummary = string.Empty;

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

    public Task OnNavigatedFromAsync()
    {
        if (_settings is not null)
        {
            _settings.Updated -= OnSettingsUpdated;
        }

        return Task.CompletedTask;
    }

    private void OnSettingsUpdated(object? sender, ShellSettings settings) => BuildChart();

    private static Color? DefaultResolveColor(string key) =>
        Application.Current?.TryFindResource(key) as Color?;

    /// <summary>
    /// Rebuilds chart paints from the live theme (1.3.0). Colors resolve
    /// from application resources with dark fallbacks, so light, dark,
    /// high-contrast, and custom accents all flow through.
    /// </summary>
    private void BuildChart()
    {
        var text = _resolveColor("TextFillColorPrimary") ?? Color.FromRgb(0xF3, 0xF3, 0xF3);
        var accent = _resolveColor("SystemAccentColorPrimary") ?? Color.FromRgb(0x4C, 0xC2, 0xFF);
        var palette = ChartPalette.For(text, accent);

        Series =
        [
            new LineSeries<double>
            {
                Values = _weeklyValues,
                Fill = null,
                Stroke = new SolidColorPaint(palette.Series, 2),
                GeometryFill = new SolidColorPaint(palette.Series),
                GeometrySize = 8,
                Name = "Completed tasks",
            },
        ];
        XAxes =
        [
            new Axis
            {
                Labels = _weekdayLabels,
                LabelsPaint = new SolidColorPaint(palette.AxisText),
                SeparatorsPaint = new SolidColorPaint(palette.GridLines),
            },
        ];
        YAxes =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(palette.AxisText),
                SeparatorsPaint = new SolidColorPaint(palette.GridLines),
            },
        ];
        ChartSummary = DescribeTrend(_weeklyValues, _weekdayLabels);
    }

    internal static string DescribeTrend(IReadOnlyList<double> values, IReadOnlyList<string> labels)
    {
        if (values.Count == 0)
        {
            return "No data.";
        }

        var total = values.Sum();
        var peak = values.Max();
        var peakIndex = 0;
        for (var i = 1; i < values.Count; i++)
        {
            if (values[i] > values[peakIndex])
            {
                peakIndex = i;
            }
        }

        var peakLabel = labels.Count == values.Count ? labels[peakIndex] : $"point {peakIndex + 1}";
        var trend = values[^1] > values[0] ? "trending up" : values[^1] < values[0] ? "trending down" : "flat";
        return $"7-day total {total:0}, peak {peakLabel} ({peak:0}), {trend}.";
    }

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
