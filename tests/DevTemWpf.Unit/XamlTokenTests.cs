using System.Text.RegularExpressions;

namespace DevTemWpf.Unit;

/// <summary>
/// 1.2.0 gap-rhythm guard: page spacing must come from AppGap* tokens so the
/// density switch changes real rhythm. Retired literals fail here on purpose.
/// Documented exceptions: 0,4,0,* optical lead-ins (density-invariant),
/// content sizes (chart heights, column widths), chrome configuration.
/// </summary>
public sealed class XamlTokenTests
{
    private static readonly string[] GapKeys =
    [
        "AppGapTitle", "AppGapItem", "AppGapGroup", "AppGapSection",
        "AppGapFlow", "AppGapInline", "AppGapGutter",
    ];

    // Every literal this milestone retired (Comfortable values).
    private static readonly string[] RetiredMargins =
    [
        "0,0,0,4", "0,0,0,8", "0,0,0,12", "0,0,0,16",
        "0,0,8,8", "0,0,8,0", "0,0,16,8", "0,0,16,0",
        "0,0,12,0", "0,12,0,0",
    ];

    private static DirectoryInfo RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        return dir;
    }

    private static Dictionary<string, string> LoadThicknessKeys(string fileName)
    {
        var path = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App", "DesignTokens", fileName);
        Assert.True(File.Exists(path), $"Missing {fileName} — run from a repo checkout.");
        var text = File.ReadAllText(path);
        return Regex.Matches(text, @"<Thickness x:Key=""(App\w+)"">(.*?)</Thickness>")
            .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value.Trim());
    }

    [Fact]
    public void GapTokens_ExistInBothDensities_AndCompactTightens()
    {
        var comfortable = LoadThicknessKeys("Spacing.Comfortable.xaml");
        var compact = LoadThicknessKeys("Spacing.Compact.xaml");

        foreach (var key in GapKeys)
        {
            Assert.True(comfortable.ContainsKey(key), $"Comfortable missing {key}.");
            Assert.True(compact.ContainsKey(key), $"Compact missing {key} — densities must define the same keys.");
        }

        // Title stays tight in both; every other gap must actually differ.
        Assert.Equal(comfortable["AppGapTitle"], compact["AppGapTitle"]);
        foreach (var key in GapKeys.Where(k => k != "AppGapTitle"))
        {
            Assert.NotEqual(comfortable[key], compact[key]);
        }
    }

    [Fact]
    public void Views_UseGapTokens_NoRetiredLiterals()
    {
        var views = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App", "Views");
        var files = Directory.GetFiles(views, "*.xaml");
        Assert.NotEmpty(files);

        var violations = new List<string>();
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            foreach (var literal in RetiredMargins)
            {
                if (text.Contains($"Margin=\"{literal}\"", StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)}: Margin=\"{literal}\"");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "Retired rhythm literals (use AppGap* instead): " + string.Join("; ", violations));
    }

    [Fact]
    public void Views_GapReferences_ResolveToDefinedKeys()
    {
        var defined = new HashSet<string>(LoadThicknessKeys("Spacing.Comfortable.xaml").Keys);
        var views = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App", "Views");
        var unknown = new List<string>();
        foreach (var file in Directory.GetFiles(views, "*.xaml"))
        {
            foreach (Match m in Regex.Matches(File.ReadAllText(file), @"\{StaticResource (AppGap\w+)\}"))
            {
                if (!defined.Contains(m.Groups[1].Value))
                {
                    unknown.Add($"{Path.GetFileName(file)}: {m.Groups[1].Value}");
                }
            }
        }

        Assert.True(unknown.Count == 0, "Unresolved gap references: " + string.Join("; ", unknown));
    }

    [Fact]
    public void Pages_KeepContentMaxWidth_ExceptDashboard()
    {
        // Dashboard is full-bleed by design (data grid needs the width);
        // every other page constrains text rhythm to AppContentMaxWidth.
        var views = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App", "Views");
        var missing = new List<string>();
        foreach (var file in Directory.GetFiles(views, "*.xaml"))
        {
            if (Path.GetFileName(file) == "DashboardPage.xaml")
            {
                continue;
            }

            if (!File.ReadAllText(file).Contains("AppContentMaxWidth", StringComparison.Ordinal))
            {
                missing.Add(Path.GetFileName(file));
            }
        }

        Assert.True(missing.Count == 0, "Pages missing AppContentMaxWidth: " + string.Join(", ", missing));
    }

    [Fact]
    public void DataGrid_KeepsRowVirtualization()
    {
        // The 10k story depends on the default virtualizing panel; fail
        // loudly if anyone disables it in XAML.
        var path = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App", "Views", "DataPage.xaml");
        var text = File.ReadAllText(path);
        Assert.DoesNotContain("EnableRowVirtualization=\"False\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("VirtualizingStackPanel.IsVirtualizing=\"False\"", text, StringComparison.Ordinal);
    }
}
