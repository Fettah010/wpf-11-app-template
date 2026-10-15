using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Resources;

namespace DevTemWpf.App.ViewModels;

/// <summary>Load state of the sample grid (drives loading/error/empty UI).</summary>
public enum DataLoadState
{
    Loading,
    Loaded,
    Error,
}

/// <summary>
/// Editable sample grid: filter/search, selection, validation, load states,
/// and a 10k-row stress toggle. Deterministic seeded data (no backend).
/// </summary>
public sealed partial class DataGridViewModel : ObservableObject
{
    public static readonly string[] Statuses =
    [
        Strings.Data_StatusOpen,
        Strings.Data_StatusBlocked,
        Strings.Data_StatusDone,
    ];

    public DataGridViewModel()
    {
        Rows = new ObservableCollection<DataGridRow>();
        View = CollectionViewSource.GetDefaultView(Rows);
        View.Filter = Matches;
        View.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(ShowEmpty));
        };
        CountText = string.Empty;
        StatusText = string.Empty;
        // Eager synchronous first load: navigation hooks (INavigationAware)
        // are not wired by the 1.x shell yet (see 1.4 roadmap), so the grid
        // must not depend on OnNavigatedToAsync to show content. Refresh
        // stays async to demonstrate the loading/error states.
        ReplaceAll(BuildRows(200, seed: 42));
        LoadState = DataLoadState.Loaded;
        UpdateCounts();
    }

    public ObservableCollection<DataGridRow> Rows { get; private set; }

    public ICollectionView View { get; private set; }

    /// <summary>
    /// Data rows currently passing the filter. Never derived from
    /// <see cref="ICollectionView.IsEmpty"/>: with CanUserAddRows the grid
    /// keeps a NewItemPlaceholder in the view, so IsEmpty is always false.
    /// </summary>
    public bool HasResults => View.Cast<object>().OfType<DataGridRow>().Any();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private DataGridRow? selectedRow;

    public string SelectedSummary => SelectedRow is null
        ? Strings.Data_SelectedNone
        : string.Format(Strings.Data_SelectedFormat, SelectedRow.Name);

    partial void OnSelectedRowChanged(DataGridRow? value) => OnPropertyChanged(nameof(SelectedSummary));

    [ObservableProperty]
    private DataLoadState loadState = DataLoadState.Loaded;

    [ObservableProperty]
    private bool simulateError;

    [ObservableProperty]
    private string countText;

    [ObservableProperty]
    private string statusText;

    private int _lastCount = 200;

    public bool IsLoading => LoadState == DataLoadState.Loading;

    public bool HasError => LoadState == DataLoadState.Error;

    /// <summary>Empty notice shows only when settled without error and nothing matches.</summary>
    public bool ShowEmpty => !IsLoading && !HasError && !HasResults;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnLoadStateChanged(DataLoadState value)
    {
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(ShowEmpty));
    }

    public static List<DataGridRow> BuildRows(int count, int seed)
    {
        var random = new Random(seed);
        var rows = new List<DataGridRow>(count);
        for (var i = 0; i < count; i++)
        {
            rows.Add(new DataGridRow(
                $"Task {i + 1:00000}",
                Statuses[random.Next(Statuses.Length)],
                random.Next(0, 10001)));
        }

        return rows;
    }

    /// <summary>Reloads the grid (async so the loading state is observable).</summary>
    public async Task RefreshAsync(int rowCount = 200)
    {
        if (LoadState == DataLoadState.Loading)
        {
            return;
        }

        _lastCount = rowCount;
        LoadState = DataLoadState.Loading;
        StatusText = Strings.Data_Loading;
        try
        {
            await Task.Delay(300).ConfigureAwait(true);
            if (SimulateError)
            {
                throw new InvalidOperationException("Simulated load failure (demo).");
            }

            ReplaceAll(BuildRows(rowCount, seed: 42));
            LoadState = DataLoadState.Loaded;
            StatusText = string.Empty;
        }
        catch (Exception ex)
        {
            LoadState = DataLoadState.Error;
            StatusText = string.Format(Strings.Data_LoadErrorFormat, ex.Message);
        }
    }

    [RelayCommand]
    private Task Refresh() => RefreshAsync(200);

    [RelayCommand]
    private Task LoadLarge() => RefreshAsync(10000);

    [RelayCommand]
    private Task Retry() => RefreshAsync(_lastCount);

    private bool Matches(object item)
    {
        if (item is not DataGridRow row)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return row.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || row.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyFilter()
    {
        View.Refresh();
        OnPropertyChanged(nameof(HasResults));
        UpdateCounts();
    }

    private void ReplaceAll(List<DataGridRow> rows)
    {
        Rows = new ObservableCollection<DataGridRow>(rows);
        OnPropertyChanged(nameof(Rows));
        View = CollectionViewSource.GetDefaultView(Rows);
        OnPropertyChanged(nameof(View));
        View.Filter = Matches;
        View.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(ShowEmpty));
        };
        SelectedRow = null;
        ApplyFilter();
    }

    private void UpdateCounts()
    {
        var shown = View.Cast<object>().OfType<DataGridRow>().Count();
        CountText = string.Format(Strings.Data_CountFormat, shown, Rows.Count);
    }
}
