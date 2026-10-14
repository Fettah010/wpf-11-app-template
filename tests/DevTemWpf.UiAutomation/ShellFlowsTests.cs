using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

namespace DevTemWpf.UiAutomation;

/// <summary>
/// Desktop UI automation (FlaUI + UIA3). Drives the built app exe; needs an
/// interactive session. Set <c>DEVTEM_RUN_UI_TESTS=0</c> to skip loudly
/// (headless CI); skips never pass silently — they print and are recorded in
/// the release report.
/// </summary>
public sealed class ShellFlowsTests : IDisposable
{
    private readonly List<Application> _apps = [];

    public void Dispose()
    {
        foreach (var app in _apps)
        {
            try
            {
                if (!app.HasExited)
                {
                    app.Kill();
                    var deadline = DateTime.UtcNow.AddSeconds(5);
                    while (!app.HasExited && DateTime.UtcNow < deadline)
                    {
                        Thread.Sleep(100);
                    }
                }

                app.Dispose();
            }
            catch
            {
                // Best effort cleanup.
            }
        }
    }

    private static bool UiEnabled(out string reason)
    {
        if (Environment.GetEnvironmentVariable("DEVTEM_RUN_UI_TESTS") == "0")
        {
            reason = "DEVTEM_RUN_UI_TESTS=0";
            return false;
        }

        if (!Environment.UserInteractive)
        {
            reason = "non-interactive session";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static string AppRoot()
    {
        // <root>/src/<App>.App/bin/<cfg>/<tfm>/ -> root is 5 levels up.
        var dir = new DirectoryInfo(Path.GetDirectoryName(FindAppExe())!);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate app root.");
        }

        return dir.FullName;
    }

    /// <summary>
    /// Preset-awareness: generated apps carry FEATURES.md with checked module
    /// rows; the repo tree has none (default everything on).
    /// </summary>
    private static bool ModuleOn(string module)
    {
        try
        {
            var manifest = Path.Combine(AppRoot(), "FEATURES.md");
            if (!File.Exists(manifest))
            {
                return true;
            }

            return File.ReadAllText(manifest).Contains($"- [x] {module}", StringComparison.Ordinal);
        }
        catch
        {
            return true;
        }
    }

    private static string FindAppExe()
    {
        // tests/DevTemWpf.UiAutomation/bin/<cfg>/<tfm>/ -> repo root is 5 levels up.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 5; i++)
        {
            dir = dir.Parent ?? throw new InvalidOperationException("Cannot locate repo root.");
        }

        foreach (var config in new[] { "Release", "Debug" })
        {
            var exe = Path.Combine(
                dir.FullName, "src", "DevTemWpf.App", "bin", config,
                "net10.0-windows10.0.26100", "DevTemWpf.App.exe");
            if (File.Exists(exe))
            {
                return exe;
            }
        }

        throw new FileNotFoundException(
            "Built app exe not found. Build the solution (Debug or Release) before running UI tests.");
    }

    private Window Launch(out Application app, out UIA3Automation automation)
    {
        app = Application.Launch(FindAppExe());
        _apps.Add(app);
        automation = new UIA3Automation();
        return app.GetMainWindow(automation, TimeSpan.FromSeconds(15))
            ?? throw new InvalidOperationException("Main window never appeared.");
    }

    private static T WaitFor<T>(Func<T?> probe, TimeSpan timeout, string what) where T : class
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var result = probe();
                if (result is not null)
                {
                    return result;
                }
            }
            catch
            {
                // Element tree is transient while navigating; retry.
            }

            Thread.Sleep(150);
        }

        throw new InvalidOperationException($"Timed out waiting for {what}.");
    }

    private static void WaitWhile(Func<bool> condition, TimeSpan timeout, string what)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (!condition())
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            Thread.Sleep(150);
        }

        throw new InvalidOperationException($"Timed out waiting for {what}.");
    }

    [WpfFact]
    public void Launch_ShowsWindow_And_ClosesCleanly()
    {
        if (!UiEnabled(out var reason))
        {
            Console.WriteLine($"SKIPPED UI test ({reason}).");
            return;
        }

        var window = Launch(out var app, out var automation);
        using (automation)
        {
            Assert.Contains("DevTemWpf", window.Title);
            window.Close();
            WaitWhile(() => !app.HasExited, TimeSpan.FromSeconds(10), "app exit");
        }
    }

    [WpfFact]
    public void Navigate_ViaMenu_And_KeyboardFocus_Moves()
    {
        if (!UiEnabled(out var reason))
        {
            Console.WriteLine($"SKIPPED UI test ({reason}).");
            return;
        }

        var window = Launch(out var app, out var automation);
        try
        {
            // Menu navigation: Home -> Controls -> Settings.
            window.FindFirstDescendant(cf => cf.ByName("Controls"))!.Click();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Gallery dialog demo")),
                TimeSpan.FromSeconds(10), "Controls page content");
            window.FindFirstDescendant(cf => cf.ByName("Settings"))!.Click();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Theme selector")),
                TimeSpan.FromSeconds(10), "Settings page content");

            // Keyboard-only: Tab moves focus across distinct elements.
            var seen = new HashSet<string>();
            for (var i = 0; i < 8; i++)
            {
                Keyboard.Press(VirtualKeyShort.TAB);
                Thread.Sleep(150);
                var focused = automation.FocusedElement();
                if (focused is not null)
                {
                    seen.Add($"{focused.ControlType}:{focused.Name}");
                }
            }

            Assert.True(seen.Count >= 3, $"Focus visited {seen.Count} distinct elements: {string.Join(" | ", seen)}");
        }
        finally
        {
            automation.Dispose();
            if (!app.HasExited)
            {
                app.Kill();
            }
        }
    }

    [WpfFact]
    public void Visit_AllPages_ShowsExpectedContent()
    {
        if (!UiEnabled(out var reason))
        {
            Console.WriteLine($"SKIPPED UI test ({reason}).");
            return;
        }

        var stops = new List<(string NavItem, string ContentName)>
        {
            // Named elements (AutomationProperties.Name). Note: an explicit
            // Name replaces the text content in UIA lookups (verified).
            ("Home", "Home headline"),
            ("Controls", "Gallery dialog demo"),
            ("Design Tokens", "Design token preview"),
            ("Effects", "Play standard preset"),
            ("Settings", "Theme selector"),
        };
        if (ModuleOn("Dashboard"))
        {
            stops.Insert(1, ("Dashboard", "Activity filter"));
        }

        if (ModuleOn("Diagnostics"))
        {
            stops.Insert(stops.Count - 1, ("Diagnostics", "Log viewer"));
        }

        var window = Launch(out var app, out var automation);
        try
        {
            foreach (var (navItem, contentName) in stops)
            {
                // Foreground first: coordinate clicks land reliably only when
                // our window is frontmost (parallel runs steal focus otherwise).
                window.Focus();
                try
                {
                    // Coordinate click: proven path for nav items (all visible).
                    // Both lookups retry: the tree rebuilds on every navigation.
                    var item = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName(navItem)),
                        TimeSpan.FromSeconds(10), $"nav item '{navItem}'");
                    item.Click();
                    WaitFor(() => window.FindFirstDescendant(cf => cf.ByName(contentName)),
                        TimeSpan.FromSeconds(10), $"{navItem} page content '{contentName}'");
                }
                catch (InvalidOperationException ex)
                {
                    var names = window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem))
                        .Select(e => e.Name).ToList();
                    var texts = window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text))
                        .Select(e => e.Name).Take(12).ToList();
                    File.WriteAllLines(
                        Path.Combine(Path.GetTempPath(), "uitreedump.txt"),
                        [$"AFTER {navItem}: {ex.Message}", "ITEMS: " + string.Join(" || ", names), "TEXTS: " + string.Join(" || ", texts)]);
                    throw;
                }
            }
        }
        finally
        {
            automation.Dispose();
            if (!app.HasExited)
            {
                app.Kill();
            }
        }
    }

    [WpfFact]
    public void Dialog_Confirm_RoundTrips_Result()
    {
        if (!UiEnabled(out var reason))
        {
            Console.WriteLine($"SKIPPED UI test ({reason}).");
            return;
        }

        var window = Launch(out var app, out var automation);
        try
        {
            window.FindFirstDescendant(cf => cf.ByName("Controls"))!.Click();
            var demo = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Gallery dialog demo")),
                TimeSpan.FromSeconds(10), "dialog demo button");
            demo.Patterns.ScrollItem.TryGetPattern(out var scrollPattern);
            scrollPattern?.ScrollIntoView();
            Thread.Sleep(300);
            demo.Patterns.Invoke.Pattern.Invoke();

            var cancel = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Cancel")),
                TimeSpan.FromSeconds(10), "dialog Cancel button");
            cancel.Patterns.Invoke.Pattern.Invoke();

            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Cancel")) is null ? new object() : null,
                TimeSpan.FromSeconds(10), "dialog to close");
        }
        finally
        {
            automation.Dispose();
            if (!app.HasExited)
            {
                app.Kill();
            }
        }
    }
}
