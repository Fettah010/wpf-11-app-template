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
        Assert.True(symbols.TryGetProperty("IncludeServices", out _));
        Assert.True(symbols.TryGetProperty("IncludeTests", out _));

        var choices = preset.GetProperty("choices").EnumerateArray()
            .Select(c => c.GetProperty("choice").GetString()).ToHashSet();
        Assert.Equal(new HashSet<string> { "Clean", "Recommended", "Full" }, choices);
        Assert.Equal("Recommended", preset.GetProperty("defaultValue").GetString());

        // Cut in 0.8.0 with reason — must not silently return.
        Assert.False(symbols.TryGetProperty("Shell", out _));
        Assert.False(symbols.TryGetProperty("ThemeDefault", out _));
    }
}
