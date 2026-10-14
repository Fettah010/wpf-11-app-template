using System.Diagnostics;
using System.Windows;
using System.Windows.Diagnostics;

namespace DevTemWpf.Integration;

/// <summary>
/// Test-only application shell: pages consume app-level token dictionaries
/// via StaticResource, which requires an <see cref="Application"/> with the
/// same merged dictionaries production loads in <c>App.xaml</c>.
/// Must be called on the test (STA) thread; safe to call repeatedly.
/// Also captures WPF data-binding errors involving our views so silent XAML
/// bugs fail tests instead of hiding (ADR-0013).
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

    private static readonly List<string> BindingErrors = [];
    private static bool _listening;

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

        if (!_listening)
        {
            _listening = true;
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ProjectBindingListener());
        }
    }

    internal static void AssertNoBindingErrors()
    {
        string[] snapshot;
        lock (BindingErrors)
        {
            snapshot = [.. BindingErrors];
        }

        Assert.Empty(snapshot);
    }

    private sealed class ProjectBindingListener : TraceListener
    {
        public override void Write(string? message)
        {
        }

        public override void WriteLine(string? message)
        {
            // Only our own views/viewmodels fail the gate; upstream control
            // internals are out of our control (recorded if seen).
            if (message is not null
                && message.Contains("DevTemWpf", StringComparison.OrdinalIgnoreCase))
            {
                lock (BindingErrors)
                {
                    BindingErrors.Add(message);
                }
            }
        }
    }
}
