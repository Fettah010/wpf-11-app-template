using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;

namespace DevTemWpf.Unit;

public sealed class DashboardViewModelTests : IDisposable
{
    private readonly string _db = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");

    public void Dispose()
    {
        foreach (var suffix in new[] { "", "-wal", "-shm", "-journal" })
        {
            try { File.Delete(_db + suffix); } catch { /* best effort */ }
        }
    }

    private DashboardViewModel Create() => new(new SampleDataService(), new FavoritesStore(_db));

    [Fact]
    public void Filter_NoMatch_ReportsEmptyState()
    {
        var vm = Create();

        vm.SearchText = "zzz-no-such-activity";

        Assert.False(vm.HasResults);
        Assert.Contains("zzz-no-such-activity", vm.NoResultsMessage);
    }

    [Fact]
    public void Filter_Category_NarrowsToMatches()
    {
        var vm = Create();

        vm.SearchText = "Network";

        var rows = vm.Activities.Cast<ActivityItem>().ToList();
        Assert.NotEmpty(rows);
        Assert.True(rows.Count < 1500);
        Assert.All(rows, a => Assert.Equal("Network", a.Category));
        Assert.True(vm.HasResults);
    }

    [Fact]
    public void Filter_Cleared_RestoresAllRows()
    {
        var vm = Create();
        vm.SearchText = "zzz-no-such-activity";

        vm.SearchText = string.Empty;

        Assert.Equal(1500, vm.Activities.Cast<ActivityItem>().Count());
    }

    [Fact]
    public void Sort_TogglesDirection_OnSameColumn()
    {
        var vm = Create();

        vm.SortByCommand.Execute("Value");
        var ascending = vm.Activities.Cast<ActivityItem>().ToList();
        Assert.True(ascending.First().Value <= ascending.Last().Value);
        Assert.Contains("↑", vm.SortStatusText);

        vm.SortByCommand.Execute("Value");
        var descending = vm.Activities.Cast<ActivityItem>().ToList();
        Assert.True(descending.First().Value >= descending.Last().Value);
        Assert.Contains("↓", vm.SortStatusText);
    }

    [Fact]
    public void Sort_UnknownColumn_FallsBackToTimestamp()
    {
        var vm = Create();

        vm.SortByCommand.Execute("Nope");

        Assert.Contains("Timestamp", vm.SortStatusText);
    }

    [Fact]
    public void Selection_HoldsPickedRow()
    {
        var vm = Create();
        var row = vm.Activities.Cast<ActivityItem>().First();

        vm.SelectedActivity = row;

        Assert.Same(row, vm.SelectedActivity);
        vm.ClearSelectionCommand.Execute(null);
        Assert.Null(vm.SelectedActivity);
    }

    [Fact]
    public void Favorite_TogglesAndPersists()
    {
        var vm = Create();
        var row = vm.Activities.Cast<ActivityItem>().First();
        vm.SelectedActivity = row;

        vm.ToggleFavoriteCommand.Execute(null);

        Assert.Contains("★", vm.FavoriteGlyph);
        Assert.True(new FavoritesStore(_db).IsFavorite(row.Id));
    }

    [Fact]
    public void Stepper_WalksForwardAndBack_WithProgress()
    {
        var vm = Create();
        Assert.Equal(0, vm.StepIndex);

        vm.NextStepCommand.Execute(null);
        vm.NextStepCommand.Execute(null);
        Assert.Equal(2, vm.StepIndex);
        Assert.Equal(100, vm.StepProgress);
        Assert.False(vm.NextStepCommand.CanExecute(null));

        vm.PreviousStepCommand.Execute(null);
        Assert.Equal(1, vm.StepIndex);
    }

    [Fact]
    public void Stepper_BoundsHold_AtEnds()
    {
        var vm = Create();

        Assert.False(vm.PreviousStepCommand.CanExecute(null));
        vm.PreviousStepCommand.Execute(null);
        Assert.Equal(0, vm.StepIndex);

        vm.NextStepCommand.Execute(null);
        vm.NextStepCommand.Execute(null);
        vm.NextStepCommand.Execute(null);
        Assert.Equal(2, vm.StepIndex);
    }

    [Fact]
    public void ChartPalette_DarkTheme_UsesLightLabels()
    {
        var palette = ChartPalette.For(
            new System.Windows.Media.Color { R = 0xF3, G = 0xF3, B = 0xF3, A = 0xFF },
            new System.Windows.Media.Color { R = 0x4C, G = 0xC2, B = 0xFF, A = 0xFF });

        Assert.True(palette.Dark);
        Assert.Equal(palette.AxisText, new SkiaSharp.SKColor(0xF3, 0xF3, 0xF3, 0xFF));
        Assert.Equal(palette.Series, new SkiaSharp.SKColor(0x4C, 0xC2, 0xFF, 0xFF));
    }

    [Fact]
    public void ChartPalette_LightTheme_UsesDarkLabels()
    {
        var palette = ChartPalette.For(
            new System.Windows.Media.Color { R = 0x1B, G = 0x1B, B = 0x1B, A = 0xFF },
            new System.Windows.Media.Color { R = 0x00, G = 0x66, B = 0xCC, A = 0xFF });

        Assert.False(palette.Dark);
        Assert.Equal(palette.AxisText, new SkiaSharp.SKColor(0x1B, 0x1B, 0x1B, 0xFF));
    }

    [Fact]
    public void ChartSummary_DescribesTotalPeakTrend()
    {
        var vm = Create();

        Assert.Contains("7-day total", vm.ChartSummary, StringComparison.Ordinal);
        Assert.Contains("peak", vm.ChartSummary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trending", vm.ChartSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Chart_RebuildsOnSettingsChange()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var settings = new SettingsService(path);
            var vm = new DashboardViewModel(
                new SampleDataService(),
                new FavoritesStore(_db),
                settings,
                _ => new System.Windows.Media.Color { R = 0x11, G = 0x11, B = 0x11, A = 0xFF });
            var first = vm.Series;

            settings.UpdateAsync(s => s with { Density = DensityMode.Compact }, TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();

            // Subscription rebuilt the chart (new paint objects) instead of
            // going stale on theme-affecting changes.
            Assert.NotSame(first, vm.Series);
            Assert.NotEmpty(vm.ChartSummary);
        }
        finally
        {
            try { File.Delete(path); } catch { /* best effort */ }
        }
    }
}
