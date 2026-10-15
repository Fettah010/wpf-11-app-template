using System.Globalization;
using System.Resources;

namespace DevTemWpf.App.Resources;

/// <summary>
/// Hand-written strongly-typed accessor (no designer codegen, so plain
/// <c>dotnet build</c> reproduces it). Backed by <c>Strings.resx</c> (+
/// satellite cultures, e.g. <c>Strings.de.resx</c>).
/// </summary>
public static class Strings
{
    private static readonly ResourceManager Manager = new(
        "DevTemWpf.App.Resources.Strings", typeof(Strings).Assembly);

    public static string Get(string key, CultureInfo? culture = null)
        => Manager.GetString(key, culture ?? CultureInfo.CurrentUICulture) ?? $"!{key}!";

    public static string Nav_Home => Get(nameof(Nav_Home));
    public static string Nav_Dashboard => Get(nameof(Nav_Dashboard));
    public static string Nav_Controls => Get(nameof(Nav_Controls));
    public static string Nav_Tokens => Get(nameof(Nav_Tokens));
    public static string Nav_Effects => Get(nameof(Nav_Effects));
    public static string Nav_Settings => Get(nameof(Nav_Settings));
    public static string Nav_Diagnostics => Get(nameof(Nav_Diagnostics));
    public static string Nav_Data => Get(nameof(Nav_Data));
    public static string Settings_Appearance => Get(nameof(Settings_Appearance));
    public static string Settings_Theme => Get(nameof(Settings_Theme));
    public static string Settings_Accent => Get(nameof(Settings_Accent));
    public static string Settings_NavigationStyle => Get(nameof(Settings_NavigationStyle));
    public static string Settings_Density => Get(nameof(Settings_Density));
    public static string Settings_Motion => Get(nameof(Settings_Motion));
    public static string Settings_Language => Get(nameof(Settings_Language));
    public static string Settings_LanguageNote => Get(nameof(Settings_LanguageNote));
    public static string Settings_RestartNow => Get(nameof(Settings_RestartNow));
}
