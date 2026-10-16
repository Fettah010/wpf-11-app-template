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

    /// <summary>
    /// Finds a Button by its visible label. Some dialog buttons surface only
    /// as inner text (no invokable peer carries the name); climb to the
    /// owning Button in that case. Prefers enabled buttons: idle gallery
    /// buttons (e.g. the busy-task Cancel) share labels with dialog buttons.
    /// Returns null when nothing actionable exists yet (keeps WaitFor honest).
    /// </summary>
    private static AutomationElement? FindButtonWithText(AutomationElement scope, string label)
    {
        var matches = scope.FindAllDescendants(cf => cf.ByName(label));
        foreach (var el in matches)
        {
            AutomationElement? button = el.ControlType == FlaUI.Core.Definitions.ControlType.Button
                ? el
                : ClimbToButton(el);
            if (button is null)
            {
                continue;
            }

            try
            {
                if (button.IsEnabled)
                {
                    return button;
                }
            }
            catch
            {
                // Stale peer; keep looking.
            }
        }

        return null;
    }

    private static AutomationElement? ClimbToButton(AutomationElement el)
    {
        var current = el.Parent;
        while (current is not null && current.ControlType != FlaUI.Core.Definitions.ControlType.Button)
        {
            current = current.Parent;
        }

        return current;
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

            var cancel = WaitFor(() =>
            {
                var button = FindButtonWithText(window, "Cancel");
                return button is not null && button.IsEnabled ? button : null;
            }, TimeSpan.FromSeconds(10), "dialog Cancel button");
            Assert.NotNull(cancel);
            cancel.Patterns.Invoke.Pattern.Invoke();

            // The busy demo button shares the "Cancel" label, so wait on the
            // dialog window itself disappearing instead of the name.
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Delete sample item?")) is null ? new object() : null,
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

            // Selecting a node selects it (programmatic Select proves the
            // selection plumbing + VM sync without depending on viewport
            // coordinates; keyboard traversal below proves interaction).
            alice.Patterns.SelectionItem.Pattern.Select();
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

            Assert.True(selected, "Selected tree node is selected.");

            // Keyboard moves selection onward (Alice -> Bob) from a focused
            // node; arrows auto-scroll, so this holds on any viewport.
            alice.Focus();
            Thread.Sleep(300);
            Keyboard.Press(VirtualKeyShort.DOWN);
            Thread.Sleep(300);
            deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            var moved = false;
            while (!moved && DateTime.UtcNow < deadline)
            {
                try
                {
                    var bob = FindTreeItem(window, "Bob");
                    moved = bob?.Patterns.SelectionItem.Pattern.IsSelected == true;
                }
                catch
                {
                    // Tree is transient; retry.
                }

                Thread.Sleep(200);
            }

            Assert.True(moved, "Keyboard DOWN moved tree selection to Bob.");

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

    /// <summary>
    /// Best-effort dropdown dismissal: selecting via SelectionItem usually
    /// auto-collapses the popup, so an explicit Collapse can race and throw.
    /// </summary>
    private static void CollapseIfExpanded(AutomationElement combo)
    {
        try
        {
            if (combo.Patterns.ExpandCollapse.Pattern.ExpandCollapseState
                == FlaUI.Core.Definitions.ExpandCollapseState.Expanded)
            {
                combo.Patterns.ExpandCollapse.Pattern.Collapse();
            }
        }
        catch
        {
            // Popup already gone; nothing to dismiss.
        }
    }

    private static void SelectShellOption(AutomationElement window, string option)
    {
        var shell = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Navigation style selector")),
            TimeSpan.FromSeconds(10), "shell selector");
        shell.Patterns.ExpandCollapse.Pattern.Expand();
        var item = WaitFor(() => shell.FindFirstDescendant(cf => cf.ByName(option)),
            TimeSpan.FromSeconds(10), $"shell option '{option}'");
        item.Patterns.SelectionItem.Pattern.Select();
        // Dismiss the dropdown: an open popup swallows the next click
        // instead of activating its target.
        CollapseIfExpanded(shell);
        Thread.Sleep(500);
    }

    /// <summary>
    /// Shell changes persist asynchronously; the restart button must only be
    /// pressed once the settings file carries the new value, otherwise the
    /// relaunched app loads stale chrome.
    /// </summary>
    private static void WaitForShellPersisted(int shellValue)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Wpf11AppTemplate", "settings.json");
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("Shell", out var shell)
                    && shell.GetInt32() == shellValue)
                {
                    return;
                }
            }
            catch
            {
                // File missing or mid-write; retry.
            }

            Thread.Sleep(200);
        }

        throw new InvalidOperationException($"Settings file never persisted Shell={shellValue}.");
    }

    /// <summary>
    /// Workspace selection persists asynchronously; wait for the settings
    /// file to carry the newly selected document before restarting, so the
    /// relaunched app cannot read a torn write (which would fall back to
    /// default settings and the wrong shell).
    /// </summary>
    private static void WaitForWorkspaceSelected(string key)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Wpf11AppTemplate", "settings.json");
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("Workspace", out var workspace)
                    && workspace.TryGetProperty("SelectedDocument", out var selected)
                    && selected.GetString() == key)
                {
                    return;
                }
            }
            catch
            {
                // File missing or mid-write; retry.
            }

            Thread.Sleep(200);
        }

        throw new InvalidOperationException($"Settings file never selected workspace document '{key}'.");
    }

    /// <summary>
    /// Clicks the in-app restart button and attaches to the relaunched
    /// process (same exe name, newer start time than the click).
    /// </summary>
    private (Window Window, Application App) RelaunchAfterRestart(
        Window window, Application app, UIA3Automation automation)
    {
        // The button carries an invariant automation name while its visible
        // content is localized.
        var restart = WaitFor(() => FindButtonWithText(window, "Restart application"),
            TimeSpan.FromSeconds(10), "restart button");
        Assert.NotNull(restart);
        var since = DateTime.UtcNow;
        var oldPid = app.ProcessId;
        restart.Patterns.Invoke.Pattern.Invoke();

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                // Exe name, not a constant: generated apps are renamed
                // (sourceName substitution), and their smoke runs this suite.
                var processName = Path.GetFileNameWithoutExtension(FindAppExe());
                var candidate = Process.GetProcessesByName(processName)
                    .Where(p => p.Id != oldPid && p.StartTime.ToUniversalTime() >= since.AddSeconds(-2))
                    .OrderByDescending(p => p.StartTime)
                    .FirstOrDefault();
                if (candidate is not null)
                {
                    var attached = Application.Attach(candidate.Id);
                    _apps.Add(attached);
                    var fresh = attached.GetMainWindow(automation, TimeSpan.FromSeconds(15));
                    if (fresh is not null)
                    {
                        return (fresh, attached);
                    }
                }
            }
            catch
            {
                // Process table is transient during restart; retry.
            }

            Thread.Sleep(500);
        }

        throw new InvalidOperationException("Relaunched app window never appeared.");
    }

    private static AutomationElement? FindTabItem(AutomationElement scope, string title)
    {
        // Tab headers surface the title on inner text; climb from the label
        // to its owning TabItem (same pattern as tree items).
        foreach (var tab in scope.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TabItem)))
        {
            try
            {
                if (tab.FindFirstDescendant(cf => cf.ByName(title)) is not null)
                {
                    return tab;
                }
            }
            catch
            {
                // Tree transient; keep looking.
            }
        }

        return null;
    }

    [WpfFact]
    public void RapidNavigation_SettlesOnLastPage()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            // Fire three navigations with no settle wait: the shell must
            // converge on the last target with content intact, never on a
            // stale page or a dead window.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item 'Controls'").Click();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data")),
                TimeSpan.FromSeconds(10), "nav item 'Data'").Click();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Effects")),
                TimeSpan.FromSeconds(10), "nav item 'Effects'").Click();

            var content = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Play standard preset")),
                TimeSpan.FromSeconds(10), "effects content after rapid navigation");
            Assert.NotNull(content);

            // And back again just as fast.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home")),
                TimeSpan.FromSeconds(10), "nav item 'Home'").Click();
            var headline = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home headline")),
                TimeSpan.FromSeconds(10), "home content after rapid navigation");
            Assert.NotNull(headline);
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

    /// <summary>Reads the detail toggle's check state (null when missing).</summary>
    private static FlaUI.Core.Definitions.ToggleState? DetailToggleState(AutomationElement scope)
    {
        try
        {
            return scope.FindFirstDescendant(cf => cf.ByName("Toggle detail pane"))
                ?.Patterns.Toggle.Pattern.ToggleState;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Reads tab header titles in tree order (proves visual reorder).
    /// </summary>
    private static List<string> TabTitles(AutomationElement scope)
    {
        var titles = new List<string>();
        foreach (var tab in scope.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TabItem)))
        {
            try
            {
                var text = tab.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));
                if (text is not null)
                {
                    titles.Add(text.Name);
                }
            }
            catch
            {
                // Tree transient; skip.
            }
        }

        return titles;
    }

    [WpfFact]
    public void WorkspaceTabs_Reorder_ByKeyboard()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            SelectShellOption(window, "Tabbed");
            WaitForShellPersisted(3); // ShellMode.Tabbed
            (window, app) = RelaunchAfterRestart(window, app, automation);

            // Three documents: home (restored), data, controls.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Open documents")),
                TimeSpan.FromSeconds(10), "documents tab strip");

            // Close everything first: persisted tabs from earlier runs would
            // otherwise make the starting order nondeterministic.
            for (var i = 0; i < 8; i++)
            {
                var any = window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TabItem))
                    .FirstOrDefault();
                if (any is null)
                {
                    break;
                }

                window.Focus();
                var closeAny = WaitFor(() => FindButtonWithText(window, "Close document"),
                    TimeSpan.FromSeconds(10), "close document button");
                Assert.NotNull(closeAny);
                closeAny.Patterns.Invoke.Pattern.Invoke();
                Thread.Sleep(500); // let close + repersist settle
            }

            // Fresh opens append in order: home, data, controls.
            // Each tab is awaited before the next click so the order is
            // deterministic regardless of page-load latency.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home")),
                TimeSpan.FromSeconds(10), "nav item 'Home'").Click();
            Assert.NotNull(WaitFor(() => FindTabItem(window, "Home"),
                TimeSpan.FromSeconds(10), "home tab"));

            // Detail pane toggles via keyboard-accessible checkbox (the
            // keyboard alternative to splitter dragging). Toggled through
            // the Toggle pattern (focus-independent on this contested
            // desktop) and verified on peer-backed children: plain Borders
            // expose no UIA peer, so the container's own name is not
            // searchable — the Close button is the reliable hook.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Toggle detail pane")),
                TimeSpan.FromSeconds(10), "detail toggle").Patterns.Toggle.Pattern.Toggle();
            WaitWhile(() => DetailToggleState(window) != FlaUI.Core.Definitions.ToggleState.Off,
                TimeSpan.FromSeconds(10), "detail toggle to uncheck");
            WaitWhile(() => FindButtonWithText(window, "Close document") is not null,
                TimeSpan.FromSeconds(10), "detail pane to collapse");
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Toggle detail pane")),
                TimeSpan.FromSeconds(10), "detail toggle").Patterns.Toggle.Pattern.Toggle();
            WaitWhile(() => DetailToggleState(window) != FlaUI.Core.Definitions.ToggleState.On,
                TimeSpan.FromSeconds(10), "detail toggle to check");
            var closeAfter = WaitFor(() => FindButtonWithText(window, "Close document"),
                TimeSpan.FromSeconds(10), "detail pane to expand");
            Assert.NotNull(closeAfter);
            Assert.NotNull(WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Detail splitter")),
                TimeSpan.FromSeconds(10), "detail splitter"));

            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data")),
                TimeSpan.FromSeconds(10), "nav item 'Data'").Click();
            Assert.NotNull(WaitFor(() => FindTabItem(window, "Data"),
                TimeSpan.FromSeconds(10), "data tab"));
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item 'Controls'").Click();
            Assert.NotNull(WaitFor(() => FindTabItem(window, "Controls"),
                TimeSpan.FromSeconds(10), "controls tab"));

            // Pointer drag was prototyped and deferred (unreliable OLE
            // gestures across automation runners; see 1.4.0 report): the
            // detail-pane Move buttons are the reorder contract and are fully
            // keyboard-operable. Move selected Controls left twice, then
            // right once, asserting exact order each time.
            window.Focus();
            var moveLeft = WaitFor(() => FindButtonWithText(window, "Move document left"),
                TimeSpan.FromSeconds(10), "move left button");
            Assert.NotNull(moveLeft);
            moveLeft.Patterns.Invoke.Pattern.Invoke();
            window.Focus();
            moveLeft = WaitFor(() => FindButtonWithText(window, "Move document left"),
                TimeSpan.FromSeconds(10), "move left button");
            Assert.NotNull(moveLeft);
            moveLeft.Patterns.Invoke.Pattern.Invoke();
            var moved = false;
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(8);
            while (!moved && DateTime.UtcNow < deadline)
            {
                try
                {
                    moved = string.Join(",", TabTitles(window)) == "Controls,Home,Data";
                }
                catch
                {
                    // Tree transient; retry.
                }

                Thread.Sleep(300);
            }

            Assert.True(moved, $"Move left did not reorder tabs: {string.Join(", ", TabTitles(window))}");

            window.Focus();
            var moveRight = WaitFor(() => FindButtonWithText(window, "Move document right"),
                TimeSpan.FromSeconds(10), "move right button");
            Assert.NotNull(moveRight);
            moveRight.Patterns.Invoke.Pattern.Invoke();
            var reordered = false;
            var reorderDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(8);
            while (!reordered && DateTime.UtcNow < reorderDeadline)
            {
                try
                {
                    reordered = string.Join(",", TabTitles(window)) == "Home,Controls,Data";
                }
                catch
                {
                    // Tree transient; retry.
                }

                Thread.Sleep(300);
            }

            Assert.True(reordered, $"Move right did not reorder tabs: {string.Join(", ", TabTitles(window))}");

            // Restore the default shell so later runs start Sidebar-first.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            SelectShellOption(window, "Sidebar");
            WaitForShellPersisted(1); // ShellMode.Sidebar
            (window, app) = RelaunchAfterRestart(window, app, automation);
            window.Focus();
            Assert.NotNull(WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home headline")),
                TimeSpan.FromSeconds(10), "home content in Sidebar shell"));
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
    public void Palette_Navigate_ByKeyboard()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            // Ctrl+K opens the palette from anywhere in the shell.
            window.Focus();
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_K);
            Keyboard.Release(VirtualKeyShort.CONTROL);
            var filter = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Command palette filter")),
                TimeSpan.FromSeconds(10), "palette filter");
            Assert.NotNull(filter);

            // Filter narrows to the Tokens page; Enter executes and closes.
            TypeIntoBox(window, "Command palette filter", "tok");
            var result = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Design Tokens")),
                TimeSpan.FromSeconds(10), "palette result");
            Assert.NotNull(result);
            Keyboard.Press(VirtualKeyShort.RETURN);
            var content = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Design token preview")),
                TimeSpan.FromSeconds(10), "tokens page after palette execute");
            Assert.NotNull(content);
            WaitWhile(() => window.FindFirstDescendant(cf => cf.ByName("Command palette filter")) is not null,
                TimeSpan.FromSeconds(10), "palette to close after execute");
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
    public void Palette_Escape_ReturnsFocus()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            // Mouse path: the header button opens with initial focus in the
            // filter box.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Open command palette")),
                TimeSpan.FromSeconds(10), "palette button").Click();
            var filter = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Command palette filter")),
                TimeSpan.FromSeconds(10), "palette filter");
            Assert.NotNull(filter);

            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            string? focused = null;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    focused = automation.FocusedElement()?.Name;
                    if (focused == "Command palette filter")
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

            Assert.Equal("Command palette filter", focused);

            // Escape dismisses and returns focus to the invoking button.
            Keyboard.Press(VirtualKeyShort.ESCAPE);
            WaitWhile(() => window.FindFirstDescendant(cf => cf.ByName("Command palette filter")) is not null,
                TimeSpan.FromSeconds(10), "palette to close after escape");

            deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            focused = null;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    focused = automation.FocusedElement()?.Name;
                    if (focused == "Open command palette")
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

            Assert.Equal("Open command palette", focused);
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
    public void TabbedShell_Documents_OpenFocusClose_And_Persist()
    {
        RequireUiOrSkip();

        var window = Launch(out var app, out var automation);
        try
        {
            // Persist Tabbed through the settings selector, then restart
            // into the workspace chrome (ADR-0015).
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            SelectShellOption(window, "Tabbed");
            WaitForShellPersisted(3); // ShellMode.Tabbed
            (window, app) = RelaunchAfterRestart(window, app, automation);

            // Workspace chrome with the restored home document.
            window.Focus();
            var tabs = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Open documents")),
                TimeSpan.FromSeconds(10), "documents tab strip");
            Assert.NotNull(tabs);
            var home = WaitFor(() => FindTabItem(window, "Home"),
                TimeSpan.FromSeconds(10), "restored home tab");
            Assert.NotNull(home);

            // Rail clicks open route-keyed documents instead of navigating.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data")),
                TimeSpan.FromSeconds(10), "nav item 'Data'").Click();
            var dataTab = WaitFor(() => FindTabItem(window, "Data"),
                TimeSpan.FromSeconds(10), "data tab");
            Assert.NotNull(dataTab);
            var grid = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid")),
                TimeSpan.FromSeconds(10), "data grid inside document");
            Assert.NotNull(grid);

            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Controls")),
                TimeSpan.FromSeconds(10), "nav item 'Controls'").Click();
            Assert.NotNull(WaitFor(() => FindTabItem(window, "Controls"),
                TimeSpan.FromSeconds(10), "controls tab"));
            Assert.NotNull(WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Gallery dialog demo")),
                TimeSpan.FromSeconds(10), "controls content inside document"));

            // Tab selection switches document content (viewport-independent).
            dataTab.Patterns.SelectionItem.Pattern.Select();
            Assert.NotNull(WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Data grid")),
                TimeSpan.FromSeconds(10), "data content after tab select"));

            // Detail pane closes the selected document.
            window.Focus();
            var close = WaitFor(() => FindButtonWithText(window, "Close document"),
                TimeSpan.FromSeconds(10), "close document button");
            Assert.NotNull(close);
            close.Patterns.Invoke.Pattern.Invoke();
            WaitWhile(() => FindTabItem(window, "Data") is not null,
                TimeSpan.FromSeconds(10), "data tab to close");

            // Remaining tabs persist across a restart (selected = last).
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            WaitForWorkspaceSelected("settings");
            SelectShellOption(window, "Tabbed");
            (window, app) = RelaunchAfterRestart(window, app, automation);
            Assert.NotNull(WaitFor(() => FindTabItem(window, "Home"),
                TimeSpan.FromSeconds(10), "persisted home tab"));
            Assert.NotNull(WaitFor(() => FindTabItem(window, "Controls"),
                TimeSpan.FromSeconds(10), "persisted controls tab"));
            // The restored selection (settings) renders its full page
            // content inside the tab frame.
            var settingsContent = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Theme selector")),
                TimeSpan.FromSeconds(10), "settings content inside restored document");
            Assert.NotNull(settingsContent);

            // Restore the default shell so later runs start Sidebar-first.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            SelectShellOption(window, "Sidebar");
            WaitForShellPersisted(1); // ShellMode.Sidebar
            (window, app) = RelaunchAfterRestart(window, app, automation);
            window.Focus();
            Assert.NotNull(WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home headline")),
                TimeSpan.FromSeconds(10), "home content in Sidebar shell"));
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
    public void TopShell_AppliesAfterRestart_And_Navigates_AllPages()
    {
        RequireUiOrSkip();

        // Shell changes apply at next launch (ADR-0015): swapping
        // PaneDisplayMode on a live WPF-UI 4.3.0 NavigationView crashes with
        // a NaN DoubleAnimationException during re-template, so the settings
        // page documents the restart requirement and this test relaunches.
        var stops = new List<(string NavItem, string ContentName)>
        {
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
            // Persist Top through the settings selector (same path a user
            // takes), then restart into the Top chrome.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            SelectShellOption(window, "Top");
            WaitForShellPersisted(2); // ShellMode.Top
            (window, app) = RelaunchAfterRestart(window, app, automation);

            foreach (var (navItem, contentName) in stops)
            {
                window.Focus();
                var item = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName(navItem)),
                    TimeSpan.FromSeconds(10), $"nav item '{navItem}' in Top shell");
                item.Click();
                var content = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName(contentName)),
                    TimeSpan.FromSeconds(10), $"{navItem} page content '{contentName}' in Top shell");
                Assert.NotNull(content);
            }

            // Restore the default shell so later runs start Sidebar-first,
            // and prove the relaunched app is back on the default chrome.
            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Settings")),
                TimeSpan.FromSeconds(10), "nav item 'Settings'").Click();
            SelectShellOption(window, "Sidebar");
            WaitForShellPersisted(1); // ShellMode.Sidebar
            (window, app) = RelaunchAfterRestart(window, app, automation);

            window.Focus();
            WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home")),
                TimeSpan.FromSeconds(10), "nav item 'Home' in Sidebar shell").Click();
            var headline = WaitFor(() => window.FindFirstDescendant(cf => cf.ByName("Home headline")),
                TimeSpan.FromSeconds(10), "home content in Sidebar shell");
            Assert.NotNull(headline);
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
