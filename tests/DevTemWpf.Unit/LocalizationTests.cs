using System.Globalization;
using DevTemWpf.App.Resources;
using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class LocalizationTests
{
    [Fact]
    public void English_IsDefault()
    {
        Assert.Equal("Home", Strings.Get("Nav_Home", CultureInfo.GetCultureInfo("en")));
        Assert.Equal("Settings", Strings.Get("Nav_Settings", CultureInfo.GetCultureInfo("en")));
    }

    [Fact]
    public void German_ResolvesFromSatellite()
    {
        Assert.Equal("Start", Strings.Get("Nav_Home", CultureInfo.GetCultureInfo("de")));
        Assert.Equal("Einstellungen", Strings.Get("Nav_Settings", CultureInfo.GetCultureInfo("de")));
    }

    [Fact]
    public void MissingKey_ReturnsMarker()
    {
        Assert.Equal("!Nope!", Strings.Get("Nope", CultureInfo.GetCultureInfo("en")));
    }

    [Fact]
    public void ApplyCulture_AcceptsEmpty_RejectsUnknown()
    {
        Assert.True(CultureService.ApplyCulture(string.Empty));
        Assert.False(CultureService.ApplyCulture("xx-invalid-culture"));
    }

    [Fact]
    public void German_CoversEveryEnglishKey_WithoutMarkers()
    {
        var en = AllKeys(CultureInfo.GetCultureInfo("en"));
        var de = AllKeys(CultureInfo.GetCultureInfo("de")).ToHashSet();
        var missing = en.Where(k => !de.Contains(k)).ToList();
        Assert.True(missing.Count == 0, "Missing de translations: " + string.Join(", ", missing));

        var markers = en
            .Select(k => (Key: k, Value: Strings.Get(k, CultureInfo.GetCultureInfo("de"))))
            .Where(t => t.Value.StartsWith("!", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(t.Value))
            .Select(t => t.Key)
            .ToList();
        Assert.True(markers.Count == 0, "Untranslated/marker de values: " + string.Join(", ", markers));
    }

    [Fact]
    public void GalleryXaml_HasNoHardcodedDisplayText()
    {
        // Display copy must come from resx ({x:Static}/{Binding}) so the UI
        // culture actually applies. Allowed: markup, punctuation/symbols/
        // numbers, and documented sample data (proper names + example values).
        // AutomationProperties.* are stable identifiers by rule — untouched.
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "Ada Lovelace",
            "ada@example.com",
        };
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        var views = Path.Combine(dir.FullName, "src", "DevTemWpf.App", "Views");
        var violations = new List<string>();
        foreach (var file in Directory.GetFiles(views, "*.xaml"))
        {
            var text = File.ReadAllText(file);
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                         text, "(?:Text|Content|Header|PlaceholderText|ToolTip|Title|HelpText|FallbackValue|Message)=\"([^\"{][^\"]*)\""))
            {
                var value = m.Groups[1].Value;
                if (allowed.Contains(value))
                {
                    continue;
                }

                if (System.Text.RegularExpressions.Regex.IsMatch(value, "[A-Za-z]{2,}"))
                {
                    violations.Add($"{Path.GetFileName(file)}: {m.Value}");
                }
            }
        }

        Assert.True(violations.Count == 0, "Hardcoded display text (use res:Strings): " + string.Join("; ", violations));
    }

    private static IEnumerable<string> AllKeys(CultureInfo culture)
    {
        // Keys are discovered from the neutral resx (single source of truth).
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        var path = Path.Combine(dir.FullName, "src", "DevTemWpf.App", "Resources", "Strings.resx");
        var doc = new System.Xml.XmlDocument();
        doc.Load(path);
        var keys = new List<string>();
        foreach (System.Xml.XmlNode node in doc.SelectNodes("/root/data")!)
        {
            var name = node.Attributes?["name"]?.Value;
            if (name is not null)
            {
                // Touch the runtime lookup too (proves resx + satellite agree).
                _ = Strings.Get(name, culture);
                keys.Add(name);
            }
        }

        return keys;
    }
}
