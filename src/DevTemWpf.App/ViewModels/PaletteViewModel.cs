using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Services;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Command palette model (1.4.0): keyboard-first route launcher with stable
/// command IDs (`nav.{route-key}`), substring filtering, and explicit
/// enabled states. The shell host injects <see cref="NavigateRoute"/> (frame
/// navigation or workspace open depending on the active shell). UI-free and
/// unit-tested; the overlay chrome lives in the shell view.
/// </summary>
public sealed partial class PaletteViewModel : ObservableObject
{
    /// <summary>A palette entry: stable ID, localized title, executability.</summary>
    public sealed record PaletteCommand(string Id, string RouteKey, Func<string> TitleSource, bool IsEnabled = true)
    {
        /// <summary>Resolved display title (culture applied at construction; language switches restart).</summary>
        public string Title => TitleSource();
    }

    public PaletteViewModel()
        : this(NavigationRoutes.All.Select(r => new PaletteCommand($"nav.{r.Key}", r.Key, r.Title)))
    {
    }

    public PaletteViewModel(IEnumerable<PaletteCommand> commands)
    {
        All = new ObservableCollection<PaletteCommand>(commands);
        RefreshResults();
    }

    /// <summary>Invoked with the route key when an enabled command executes.</summary>
    public Action<string>? NavigateRoute { get; set; }

    public ObservableCollection<PaletteCommand> All { get; }

    public ObservableCollection<PaletteCommand> Results { get; } = [];

    [ObservableProperty]
    private string query = string.Empty;

    [ObservableProperty]
    private PaletteCommand? selectedResult;

    /// <summary>Executes the selected command when it is enabled.</summary>
    [RelayCommand]
    private void ExecuteSelected()
    {
        if (SelectedResult is { IsEnabled: true } command)
        {
            NavigateRoute?.Invoke(command.RouteKey);
        }
    }

    /// <summary>Moves selection by delta, clamping to enabled entries.</summary>
    public void MoveSelection(int delta)
    {
        if (Results.Count == 0)
        {
            return;
        }

        var index = SelectedResult is null ? (delta > 0 ? -1 : Results.Count) : Results.IndexOf(SelectedResult);
        index = Math.Clamp(index + delta, 0, Results.Count - 1);
        SelectedResult = Results[index];
    }

    partial void OnQueryChanged(string value) => RefreshResults();

    private void RefreshResults()
    {
        Results.Clear();
        foreach (var command in All.Where(c => Matches(c, Query)))
        {
            Results.Add(command);
        }

        SelectedResult = Results.FirstOrDefault(c => c.IsEnabled);
    }

    private static bool Matches(PaletteCommand command, string query)
        => string.IsNullOrWhiteSpace(query)
            || command.Title.Contains(query, StringComparison.OrdinalIgnoreCase);
}
