using System.Windows;

namespace DevTemWpf.Integration;

/// <summary>
/// Test-only application shell: pages consume app-level token dictionaries
/// via StaticResource, which requires an <see cref="Application"/> with the
/// same merged dictionaries production loads in <c>App.xaml</c>.
/// Must be called on the test (STA) thread; safe to call repeatedly.
/// </summary>
internal static class TestResources
{
    private static readonly string[] Dictionaries =
    [
        "DesignTokens/SemanticColors.xaml",
        "DesignTokens/Typography.xaml",
        "DesignTokens/Spacing.Comfortable.xaml",
        "DesignTokens/Shapes.xaml",
    ];

    internal static void Ensure()
    {
        var app = Application.Current ?? new Application();
        foreach (var file in Dictionaries)
        {
            var uri = $"/DevTemWpf.App;component/{file}";
            var already = app.Resources.MergedDictionaries.Any(d =>
                string.Equals(d.Source?.OriginalString, uri, StringComparison.OrdinalIgnoreCase));
            if (!already)
            {
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri(uri, UriKind.Relative),
                });
            }
        }
    }
}
