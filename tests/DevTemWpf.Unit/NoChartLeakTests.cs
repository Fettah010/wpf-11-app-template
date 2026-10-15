namespace DevTemWpf.Unit;

/// <summary>
/// 1.3.0 package-boundary guard: LiveCharts/SkiaSharp stay dashboard-only
/// (Full preset). Clean and Recommended must not reference, ship, or load
/// charting — verified here at source level plus by the template matrix
/// module checks (Velopack/Serilog/LiveCharts refs per preset).
/// </summary>
public sealed class NoChartLeakTests
{
    private static readonly string[] AllowedFiles =
    [
        "DashboardPage.xaml",
        "DashboardPage.xaml.cs",
        "DashboardViewModel.cs",
        "ChartPalette.cs",
    ];

    private static readonly string[] DashboardExcludedFromNonFull =
    [
        "src/DevTemWpf.App/Views/DashboardPage.xaml",
        "src/DevTemWpf.App/Views/DashboardPage.xaml.cs",
        "src/DevTemWpf.App/ViewModels/DashboardViewModel.cs",
        "src/DevTemWpf.App/ViewModels/ChartPalette.cs",
    ];

    private static readonly string[] ChartMarkers =
    [
        "LiveCharts",
        "SkiaSharp",
        "lvc:",
        "CartesianChart",
        "ChartPalette",
    ];

    [Fact]
    public void Charting_StaysDashboardOnly()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        var app = Path.Combine(dir.FullName, "src", "DevTemWpf.App");
        var leaks = new List<string>();
        foreach (var file in Directory.GetFiles(app, "*.*", SearchOption.AllDirectories)
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                         && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                         && (f.EndsWith(".cs", StringComparison.Ordinal)
                             || f.EndsWith(".xaml", StringComparison.Ordinal))))
        {
            if (AllowedFiles.Contains(Path.GetFileName(file), StringComparer.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            foreach (var marker in ChartMarkers)
            {
                if (text.Contains(marker, StringComparison.Ordinal))
                {
                    leaks.Add($"{Path.GetFileName(file)}: {marker}");
                }
            }
        }

        Assert.True(leaks.Count == 0, "Charting leaked outside dashboard files: " + string.Join("; ", leaks));
    }

    [Fact]
    public void TemplateJson_ExcludesChartFiles_FromNonFullPresets()
    {
        // Regression guard for 1.3.0: ChartPalette.cs shipped in Clean and
        // broke its build (SkiaSharp only exists with the Full chart package).
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        var path = Path.Combine(dir.FullName, ".template.config", "template.json");
        using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
        var modifiers = doc.RootElement.GetProperty("sources")[0].GetProperty("modifiers");
        var dashboard = modifiers.EnumerateArray()
            .First(m => m.GetProperty("condition").GetString() == "(!IncludeDashboard)");
        var excluded = dashboard.GetProperty("exclude").EnumerateArray()
            .Select(e => e.GetString() ?? string.Empty).ToHashSet();
        var missing = DashboardExcludedFromNonFull.Where(f => !excluded.Contains(f)).ToList();
        Assert.True(missing.Count == 0, "Not excluded from non-Full presets: " + string.Join(", ", missing));
    }
}
