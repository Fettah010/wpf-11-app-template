using System.Diagnostics;
using System.Runtime.InteropServices;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

namespace DevTemWpf.UiAutomation;

/// <summary>
/// Desktop UI automation (FlaUI + UIA3). Drives the built app exe; needs an
/// interactive session. Set <c>DEVTEM_RUN_UI_TESTS=0</c> to skip loudly
/// (headless CI). Skips use <c>Assert.Skip</c> so they are counted as skipped
/// (never as passes); the required release gate fails on unexpected skips —
/// see <c>eng/check-ui-tests.ps1</c>.
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

    private static class NativeWindow
    {
        public const int Restore = 9;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public readonly int Width => Right - Left;
            public readonly int Height => Bottom - Top;
        }
    }

    /// <summary>
    /// Sets a named text box exactly like a user would (focus — which scrolls
    /// it into view — select all, type), re-finding the box on every attempt
    /// and verifying the effect by reading the value back. Retries while
    /// desktop focus is contested; throws on persistent mismatch.
    /// </summary>
    private static void TypeIntoBox(Window window, string boxName, string text)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var box = window.FindFirstDescendant(cf => cf.ByName(boxName));
                if (box is null)
                {
                    Thread.Sleep(300);
                    continue;
                }

                box.Focus();
                Thread.Sleep(200);
                Keyboard.Press(VirtualKeyShort.CONTROL);
                Keyboard.Press(VirtualKeyShort.KEY_A);
                Keyboard.Release(VirtualKeyShort.CONTROL);
                Keyboard.Press(VirtualKeyShort.BACK);
                foreach (var ch in text)
                {
                    Keyboard.Type(ch.ToString());
                    Thread.Sleep(30);
                }

                Thread.Sleep(500);
                var current = window.FindFirstDescendant(cf => cf.ByName(boxName));
                if (current?.Patterns.Value.Pattern.Value == text)
                {
                    return;
                }
            }
            catch
            {
                // Tree transient; retry.
            }

            Thread.Sleep(300);
        }

        throw new InvalidOperationException($"Could not set '{boxName}' to '{text}' (desktop focus may be contested).");
    }

    private static AutomationElement? FindTreeItem(AutomationElement scope, string label)
    {
        // TreeViewItem containers are unnamed (the label lives on the inner
        // text); climb from the label to its owning TreeItem.
        var text = scope.FindAllDescendants(cf => cf.ByName(label))
            .FirstOrDefault(e => e.ControlType == FlaUI.Core.Definitions.ControlType.Text);
        var current = text?.Parent;
        while (current is not null && current.ControlType != FlaUI.Core.Definitions.ControlType.TreeItem)
        {
            current = current.Parent;
        }

        return current;
    }

    private static IntPtr MainHandle(Application app)
    {
        var handle = Process.GetProcessById(app.ProcessId).MainWindowHandle;
        Assert.NotEqual(IntPtr.Zero, handle);
        return handle;
    }

    private static void RequireUiOrSkip()
    {
        if (!UiEnabled(out var reason))
        {
            Assert.Skip($"UI tests disabled ({reason}). Run on an interactive Windows session without DEVTEM_RUN_UI_TESTS=0.");
        }
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
        var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(15))
            ?? throw new InvalidOperationException("Main window never appeared.");
        // Cover mitigation: maximize + foreground so coordinate clicks land
        // on our window even when another maximized window (or a busy
        // desktop) is present. Layout assertions are unaffected.
        if (window.Patterns.Window.TryGetPattern(out var windowPattern))
        {
            windowPattern.SetWindowVisualState(FlaUI.Core.Definitions.WindowVisualState.Maximized);
        }

        window.Focus();
        // Best-effort foreground: on contested desktops another window can
        // steal activation between steps. Bounded wait; never fails here —
        // later waits fail loudly with context if clicks truly miss.
        try
        {
            var me = Process.GetProcessById(app.ProcessId).MainWindowHandle;
            var until = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (DateTime.UtcNow < until && NativeWindow.GetForegroundWindow() != me)
            {
                window.Focus();
                Thread.Sleep(250);
            }
        }
        catch
        {
            // Best effort only.
        }

        return window;
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
        RequireUiOrSkip();

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
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            // Menu navigation: Home -> Controls -> Settings.
            // Foreground first: coordinate clicks land reliably only when
            // our window is frontmost (same mitigation as Visit_AllPages).
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item 'Controls'").Click();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Gallery dialog demo")),
                TimeSpan.FromSeconds(10), "Controls page content");
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
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
        RequireUiOrSkip();

        var stops = new List<(string NavItem, string ContentName)>
        {
            // Named elements (AutomationProperties.Name). Note: an explicit
            // Name replaces the text content in UIA lookups (verified).
            ("Home", "Home headline"),
            ("Controls", "Gallery dialog demo"),
            ("Data", "Data grid"),
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
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item 'Controls'").Click();
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

    [WpfFact]
    public void GalleryIndex_JumpsToSection_AndMovesFocus()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item 'Controls'").Click();
            var index = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Go to Inputs section")),
                TimeSpan.FromSeconds(10), "gallery index button");
            index.Patterns.Invoke.Pattern.Invoke();

            var heading = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Inputs section heading")),
                TimeSpan.FromSeconds(10), "inputs section heading");
            Assert.NotNull(heading);

            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            string? focused = null;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    focused = automation.FocusedElement()?.Name;
                    if (focused == "Inputs section heading")
                    {
                        break;
                    }
                }
                catch
                {
                    // Focused element is transient; retry.
                }

                Thread.Sleep(150);
            }

            Assert.Equal("Inputs section heading", focused);
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
    public void DataGrid_FilterSelectKeyboard_RoundTrip()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data")),
                TimeSpan.FromSeconds(10), "nav item 'Data'").Click();
            var grid = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid")),
                TimeSpan.FromSeconds(10), "data grid");
            Assert.NotNull(grid);

            // Filter narrows the rows; the surviving cell proves it. (Label
            // TextBlocks carry automation names instead of content, so the
            // test asserts on cell presence, not label text.)
            var filter = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid filter")),
                TimeSpan.FromSeconds(10), "data grid filter");
            TypeIntoBox(window, "Data grid filter", "Task 00001");
            var cell = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Task 00001")),
                TimeSpan.FromSeconds(10), "filtered cell");
            Assert.NotNull(cell);

            // Impossible filter shows the empty notice (collapsed = absent
            // from the UIA tree, so presence proves visibility).
            filter = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid filter")),
                TimeSpan.FromSeconds(10), "data grid filter");
            TypeIntoBox(window, "Data grid filter", "zzz-no-match");
            var empty = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid empty notice")),
                TimeSpan.FromSeconds(10), "empty notice");
            Assert.NotNull(empty);

            // Clearing restores rows; selecting the first one selects it.
            window = app.GetMainWindow(automation, TimeSpan.FromSeconds(15))
                ?? throw new InvalidOperationException("Main window lost.");
            window.Focus();
            filter = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid filter")),
                TimeSpan.FromSeconds(10), "data grid filter");
            TypeIntoBox(window, "Data grid filter", string.Empty);
            cell = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Task 00001")),
                TimeSpan.FromSeconds(10), "restored cell");
            cell.Click();
            // Selection is asserted on the grid: in FullRow mode cells do not
            // carry SelectionItem themselves.
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            var selected = false;
            while (!selected && DateTime.UtcNow < deadline)
            {
                try
                {
                    selected = grid.Patterns.Selection.Pattern.Selection.Value.Length > 0;
                }
                catch
                {
                    // Tree is transient; retry.
                }

                Thread.Sleep(200);
            }

            Assert.True(selected, "Clicked grid row is selected.");

            // Keyboard reaches grid content (focus moves across distinct elements).
            filter = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid filter")),
                TimeSpan.FromSeconds(10), "data grid filter");
            filter.Click();
            var seen = new HashSet<string>();
            for (var i = 0; i < 10; i++)
            {
                Keyboard.Press(VirtualKeyShort.TAB);
                Thread.Sleep(150);
                var focused = automation.FocusedElement();
                if (focused is not null)
                {
                    seen.Add($"{focused.ControlType}:{focused.Name}");
                }
            }

            Assert.True(seen.Count >= 3, $"Focus visited {seen.Count} distinct elements.");
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
    public void DataTree_ExpandSelectKeyboard_RoundTrip()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data")),
                TimeSpan.FromSeconds(10), "nav item 'Data'").Click();
            var tree = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data tree")),
                TimeSpan.FromSeconds(10), "data tree");
            Assert.NotNull(tree);

            // Expand-all reveals deep nodes. Note: nodes surface twice in the
            // tree (TreeItem container + inner text); interact with the
            // TreeItem, which carries SelectionItem.
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Expand all nodes")),
                TimeSpan.FromSeconds(10), "expand-all button").Patterns.Invoke.Pattern.Invoke();
            var alice = WaitFor(() => FindTreeItem(window, "Alice"),
                TimeSpan.FromSeconds(10), "tree leaf");
            Assert.NotNull(alice);

            // Selecting a node selects it. Keyboard-driven from a visible node:
            // arrow navigation auto-scrolls, so no manual scrolling is needed.
            var engineering = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Engineering")),
                TimeSpan.FromSeconds(10), "tree root label");
            engineering.Click();
            Thread.Sleep(300);
            Keyboard.Press(VirtualKeyShort.DOWN); // Engineering -> Backend
            Thread.Sleep(300);
            Keyboard.Press(VirtualKeyShort.RIGHT); // Backend -> first child (Alice)
            Thread.Sleep(300);
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            var selected = false;
            while (!selected && DateTime.UtcNow < deadline)
            {
                try
                {
                    var current = FindTreeItem(window, "Alice");
                    selected = current?.Patterns.SelectionItem.Pattern.IsSelected == true;
                }
                catch
                {
                    // Tree is transient; retry.
                }

                Thread.Sleep(200);
            }

            Assert.True(selected, "Keyboard-selected tree node is selected.");

            // Keyboard traverses the tree onward from Alice (Bob, Cara, …).
            // TreeItems are unnamed, so key on each focused item's label text.
            string LabelOf(AutomationElement? el) =>
                el?.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text))?.Name ?? "<none>";
            var seen = new HashSet<string>();
            for (var i = 0; i < 6; i++)
            {
                Keyboard.Press(VirtualKeyShort.DOWN);
                Thread.Sleep(200);
                seen.Add(LabelOf(automation.FocusedElement()));
            }

            Assert.True(seen.Count >= 2, $"Keyboard visited {seen.Count} distinct tree elements.");
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
    public void Resize_PreservesNavigationAndContent()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            // Launch maximizes; restore first so resizes take effect.
            var handle = MainHandle(app);
            NativeWindow.ShowWindow(handle, NativeWindow.Restore);
            Thread.Sleep(500);

            // Below-minimum requests clamp to MinWidth/MinHeight (640x480).
            NativeWindow.MoveWindow(handle, 0, 0, 500, 400, true);
            Thread.Sleep(800);
            NativeWindow.GetWindowRect(handle, out var clamped);
            Assert.True(clamped.Width >= 640, $"width clamped to min, got {clamped.Width}");
            Assert.True(clamped.Height >= 480, $"height clamped to min, got {clamped.Height}");

            foreach (var width in new[] { 640, 1024, 1280, 1600 })
            {
                NativeWindow.MoveWindow(handle, 0, 0, width, 800, true);
                Thread.Sleep(800);
                window.Focus();
                var nav = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                    TimeSpan.FromSeconds(10), $"nav item at width {width}");
                Assert.NotNull(nav);
                var home = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home headline")),
                    TimeSpan.FromSeconds(10), $"home content at width {width}");
                Assert.NotNull(home);
            }

            // Narrow navigation still works: gallery content reachable at 640.
            NativeWindow.MoveWindow(handle, 0, 0, 640, 800, true);
            Thread.Sleep(800);
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item at 640").Click();
            var gallery = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Gallery dialog demo")),
                TimeSpan.FromSeconds(10), "gallery content at 640");
            Assert.NotNull(gallery);
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
    public void ThemeSwitch_LightDark_KeepsContent()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            var combo = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Theme selector")),
                TimeSpan.FromSeconds(10), "theme selector");
            Assert.NotNull(combo);

            foreach (var theme in new[] { "Light", "Dark" })
            {
                combo.Patterns.ExpandCollapse.Pattern.Expand();
                var item = WaitFor(() => combo.FindFirstDescendant(cf => cf.ByName(theme)),
                    TimeSpan.FromSeconds(10), $"theme option '{theme}'");
                item.Patterns.SelectionItem.Pattern.Select();
                // Dismiss the dropdown: an open popup swallows the next click
                // instead of activating its target.
                combo.Patterns.ExpandCollapse.Pattern.Collapse();
                Thread.Sleep(1000); // Theme re-resolves live; let it settle.
                Assert.NotNull(window.FindFirstDescendant(cf => cf.ByName("Theme selector")));

                window.Focus();
                WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home")),
                    TimeSpan.FromSeconds(10), $"nav item after '{theme}'").Click();
                var headline = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home headline")),
                    TimeSpan.FromSeconds(10), $"home content on '{theme}'");
                Assert.NotNull(headline);

                window.Focus();
                WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                    TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
                combo = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Theme selector")),
                    TimeSpan.FromSeconds(10), "theme selector");
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
    public void TimeEditor_SetAndValidate_RoundTrip()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data")),
                TimeSpan.FromSeconds(10), "nav item 'Data'").Click();
            var hour = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Time hour input")),
                TimeSpan.FromSeconds(10), "time hour input");
            Assert.NotNull(hour);

            // Invalid hour surfaces a text error (collapsed = absent, so
            // presence proves visibility).
            TypeIntoBox(window, "Time hour input", "99");
            var error = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Time validation error")),
                TimeSpan.FromSeconds(10), "time validation error");
            Assert.NotNull(error);

            // Valid input clears the error and updates the summary.
            TypeIntoBox(window, "Time hour input", "14");
            TypeIntoBox(window, "Time minute input", "30");
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            var cleared = false;
            while (!cleared && DateTime.UtcNow < deadline)
            {
                Thread.Sleep(300);
                cleared = window.FindFirstDescendant(cf => cf.ByName("Time validation error")) is null;
            }

            Assert.True(cleared, "Time error cleared after valid input.");
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
    public void BusyDemo_Cancel_StopsEarly()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item 'Controls'").Click();
            var run = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Gallery busy demo button")),
                TimeSpan.FromSeconds(10), "busy demo button");
            run.Patterns.Invoke.Pattern.Invoke();

            // Cancel while busy: the run button must re-enable long before
            // the 5-second natural finish.
            var cancel = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Cancel busy task")),
                TimeSpan.FromSeconds(10), "cancel busy button");
            cancel.Patterns.Invoke.Pattern.Invoke();
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(4);
            var idle = false;
            while (!idle && DateTime.UtcNow < deadline)
            {
                try
                {
                    idle = window.FindFirstDescendant(cf => cf.ByName("Gallery busy demo button"))?.IsEnabled == true;
                }
                catch
                {
                    // Tree transient; retry.
                }

                Thread.Sleep(200);
            }

            Assert.True(idle, "Run button re-enabled shortly after cancel (not after 5 s).");
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
