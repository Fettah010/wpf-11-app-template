using System.Diagnostics;
using System.Windows;
using System.Windows.Diagnostics;

namespace DevTemWpf.Integration;

/// <summary>
/// Test-only application shell: use the real app resources so tests exercise
/// the same merged UI-library and WPF-UI dictionaries as production.
/// Must be called on the test (STA) thread; safe to call repeatedly.
/// Also captures WPF data-binding errors involving our views so silent XAML
/// bugs fail tests instead of hiding (ADR-0013).
/// </summary>
internal static class TestResources
{
    private static readonly List<string> BindingErrors = [];
    private static bool _listening;

    internal static void Ensure()
    {
        var app = Application.Current;
        if (app is null)
        {
            var application = new DevTemWpf.App.App();
            application.InitializeComponent();
            app = application;
        }

        if (app is not DevTemWpf.App.App)
        {
            throw new InvalidOperationException("Integration tests require the production App resource dictionaries.");
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
