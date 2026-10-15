using System.Text.RegularExpressions;

namespace DevTemWpf.Unit;

/// <summary>
/// 1.2.0 theme-chain guard: every color in app XAML must flow through the
/// App* alias layer (or an explicitly approved upstream WPF-UI key), so
/// light/dark/high-contrast switches propagate. A new hard-coded color or
/// an unreviewed upstream key fails here on purpose. Contrast pairs are
/// documented in docs/design-system/tokens.md; numeric review is manual
/// (see 1.2.0 report) because upstream values live in the theme dictionaries.
/// </summary>
public sealed class ThemeTokenChainTests
{
    private static readonly HashSet<string> ApprovedUpstream = new(StringComparer.Ordinal)
    {
        "TextFillColorPrimary", "TextFillColorSecondary", "TextFillColorTertiary", "TextFillColorDisabled",
        "TextOnAccentFillColorPrimary",
        "SystemAccentColorPrimary", "SystemAccentColorSecondary",
        "CardBackgroundFillColorDefault", "SubtleFillColorSecondary",
        "SolidBackgroundFillColorBase", "SolidBackgroundFillColorTertiary",
        "CardStrokeColorDefault", "DividerStrokeColorDefault",
        "FocusStrokeColorOuter", "FocusStrokeColorInner",
        "SystemFillColorSuccess", "SystemFillColorCaution", "SystemFillColorCritical",
        "SystemFillColorNeutral", "SystemFillColorCriticalBackground",
    };

    private static DirectoryInfo RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        return dir;
    }

    private static IEnumerable<string> AppXamlFiles()
    {
        var app = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App");
        return Directory.GetFiles(app, "*.xaml", SearchOption.AllDirectories);
    }

    private static IEnumerable<string> DynamicResourceKeys(string text)
    {
        foreach (Match m in Regex.Matches(text, @"\{DynamicResource (\w+)\}"))
        {
            yield return m.Groups[1].Value;
        }
    }

    [Fact]
    public void AliasLayer_OnlyApprovedUpstream()
    {
        var path = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App", "DesignTokens", "SemanticColors.xaml");
        var bad = DynamicResourceKeys(File.ReadAllText(path)).Where(k => !ApprovedUpstream.Contains(k)).ToList();
        Assert.True(bad.Count == 0, "Unapproved upstream keys in SemanticColors.xaml: " + string.Join(", ", bad));
    }

    [Fact]
    public void AppXaml_OnlyApprovedOrAlias()
    {
        var bad = new List<string>();
        foreach (var file in AppXamlFiles())
        {
            foreach (var key in DynamicResourceKeys(File.ReadAllText(file)))
            {
                if (!key.StartsWith("App", StringComparison.Ordinal) && !ApprovedUpstream.Contains(key))
                {
                    bad.Add($"{Path.GetFileName(file)}: {key}");
                }
            }
        }

        Assert.True(bad.Count == 0, "Unapproved color references: " + string.Join("; ", bad));
    }

    [Fact]
    public void ViewsAndTokens_NoLiteralColors()
    {
        var bad = new List<string>();
        foreach (var dir in new[] { "Views", "DesignTokens" })
        {
            var path = Path.Combine(RepoRoot().FullName, "src", "DevTemWpf.App", dir);
            foreach (var file in Directory.GetFiles(path, "*.xaml"))
            {
                foreach (Match m in Regex.Matches(File.ReadAllText(file), @"Color=""#[0-9A-Fa-f]+"""))
                {
                    bad.Add($"{Path.GetFileName(file)}: {m.Value}");
                }
            }
        }

        Assert.True(bad.Count == 0, "Hard-coded colors (use App* tokens): " + string.Join("; ", bad));
    }
}
