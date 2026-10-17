using System.Text.Json;

namespace DevTemWpf.Unit;

/// <summary>
/// 1.0 surface freeze guard (ADR: frozen-surface.md). Any template-option
/// rename/removal breaks this test on purpose — update docs + cut a new rc.
/// </summary>
public sealed class TemplateSurfaceTests
{
    private static JsonDocument LoadTemplateJson()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        var path = Path.Combine(dir.FullName, ".template.config", "template.json");
        Assert.True(File.Exists(path), "template.json not found — run from a repo checkout.");
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    [Fact]
    public void AppTemplate_Surface_IsFrozen()
    {
        using var doc = LoadTemplateJson();
        var root = doc.RootElement;

        Assert.Equal("devtemwpf-app", root.GetProperty("shortName").GetString());
        Assert.Equal("DevTemWpf", root.GetProperty("sourceName").GetString());

        var symbols = root.GetProperty("symbols");
        Assert.True(symbols.TryGetProperty("Preset", out var preset));
        Assert.True(symbols.TryGetProperty("IncludeDashboard", out _));
        Assert.True(symbols.TryGetProperty("IncludeDiagnostics", out _));
        Assert.True(symbols.TryGetProperty("IncludeTests", out _));

        // 1.6.0 (roadmap §8.6): the single IncludeServices flag is replaced by
        // per-module flags; each module is independently selectable.
        foreach (var module in new[]
                 {
                     "IncludeLogging", "IncludeLocalData", "IncludeHttp",
                     "IncludeBackup", "IncludeUpdates", "IncludeTray",
                     "IncludeAuth", "IncludeCrash",
                 })
        {
            Assert.True(symbols.TryGetProperty(module, out _), $"{module} symbol missing");
        }

        foreach (var custom in new[]
                 {
                     "CustomLogging", "CustomLocalData", "CustomHttp",
                     "CustomBackup", "CustomUpdates", "CustomTray", "CustomDiagnostics",
                     "CustomAuth", "CustomCrash",
                 })
        {
            Assert.True(symbols.TryGetProperty(custom, out _), $"{custom} parameter missing");
        }

        // Removed in 1.6.0 — must not silently return.
        Assert.False(symbols.TryGetProperty("IncludeServices", out _));

        var choices = preset.GetProperty("choices").EnumerateArray()
            .Select(c => c.GetProperty("choice").GetString() ?? string.Empty).ToHashSet();
        Assert.Equal(new HashSet<string> { "Clean", "Recommended", "Full", "Custom" }, choices);
        Assert.Equal("Recommended", preset.GetProperty("defaultValue").GetString());

        // Added in 1.4.0 (roadmap §8.4, ADR-0015): additive scaffold-time
        // shell default. The choice value replaces the DevTemWpfShellDefault
        // token in ShellSettings; all shells ship in every scaffold.
        Assert.True(symbols.TryGetProperty("Shell", out var shell));
        var shellChoices = shell.GetProperty("choices").EnumerateArray()
            .Select(c => c.GetProperty("choice").GetString() ?? string.Empty).ToHashSet();
        Assert.Equal(new HashSet<string> { "Sidebar", "Rail", "Top", "Tabbed" }, shellChoices);
        Assert.Equal("Sidebar", shell.GetProperty("defaultValue").GetString());
        Assert.Equal("DevTemWpfShellDefault", shell.GetProperty("replaces").GetString());

        // Cut in 0.8.0 with reason — must not silently return.
        Assert.False(symbols.TryGetProperty("ThemeDefault", out _));
    }
}
