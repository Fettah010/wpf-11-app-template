using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.Unit;

/// <summary>Workspace contract (1.4.0): route-keyed documents, stale-guard, persistence.</summary>
public sealed class WorkspaceViewModelTests : IDisposable
{
    private sealed class HookViewModel : ObservableObject, INavigationAware
    {
        public int Entered { get; private set; }

        public int Left { get; private set; }

        public Task OnNavigatedToAsync()
        {
            Entered++;
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            Left++;
            return Task.CompletedTask;
        }
    }

    private sealed class HookPage : Page
    {
        public HookPage(HookViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
        }

        public HookViewModel ViewModel { get; }
    }

    private sealed class BlockingViewModel : ObservableObject, INavigationAware
    {
        private readonly Task _gate;

        public BlockingViewModel(Task gate)
        {
            _gate = gate;
        }

        public async Task OnNavigatedToAsync() => await _gate.ConfigureAwait(false);

        public Task OnNavigatedFromAsync() => Task.CompletedTask;
    }

    private sealed class BlockingPage : Page
    {
        public BlockingPage(Task gate)
        {
            ViewModel = new BlockingViewModel(gate);
            DataContext = this;
        }

        public BlockingViewModel ViewModel { get; }
    }

    private readonly string _settingsPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
    private readonly HookViewModel _hooks = new();

    public void Dispose()
    {
        try { File.Delete(_settingsPath); } catch { /* best effort */ }
    }

    private WorkspaceViewModel Create(
        Func<Type, object>? factory = null,
        ShellSettings? seed = null)
    {
        var settings = new SettingsService(_settingsPath);
        if (seed is not null)
        {
            settings.UpdateAsync(_ => seed, TestContext.Current.CancellationToken).GetAwaiter().GetResult();
        }

        return new WorkspaceViewModel(
            settings,
            factory ?? (t => t == typeof(HookPage) ? new HookPage(_hooks) : new Page()));
    }

    [WpfFact]
    public void Restore_OpensPersistedDocs_And_SelectsPersisted()
    {
        var vm = Create(seed: new ShellSettings
        {
            Workspace = new WorkspaceState { OpenDocuments = ["home", "home"], SelectedDocument = "home" },
        });
        vm.EnsureLoaded();

        Assert.Single(vm.Documents);
        Assert.Equal("home", vm.SelectedDocument?.RouteKey);
    }

    [WpfFact]
    public async Task Restore_IgnoresUnknownKeys_And_FallsBackToHome()
    {
        var vm = Create(seed: new ShellSettings
        {
            Workspace = new WorkspaceState { OpenDocuments = ["nope", "also-nope"], SelectedDocument = "nope" },
        });
        vm.EnsureLoaded();

        Assert.Single(vm.Documents);
        Assert.Equal("home", vm.SelectedDocument?.RouteKey);
        await Task.CompletedTask;
    }

    [WpfFact]
    public async Task Open_FocusesExisting_InsteadOfDuplicating()
    {
        var opens = 0;
        var vm = Create(t => { opens++; return new Page(); });
        await vm.OpenDocumentAsync("home");
        await vm.OpenDocumentAsync("home");

        Assert.Single(vm.Documents);
        Assert.Equal(1, opens);
    }

    [WpfFact]
    public async Task Open_RunsEnterHook_And_Close_RunsLeaveHook()
    {
        var vm = Create(t => new HookPage(_hooks));
        await vm.OpenDocumentAsync("controls");

        Assert.Equal(1, _hooks.Entered);
        await vm.CloseDocumentAsync(vm.Documents[0]);
        Assert.Equal(1, _hooks.Left);
    }

    [WpfFact]
    public async Task CancelledOpen_NeverSurfaces()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = Create(
            t => new BlockingPage(gate.Task),
            seed: new ShellSettings
            {
                Workspace = new WorkspaceState { OpenDocuments = ["controls"] },
            });
        vm.EnsureLoaded();
        Assert.Single(vm.Documents);

        using var cts = new CancellationTokenSource();
        var open = vm.OpenDocumentAsync("home", cts.Token);
        cts.Cancel();
        gate.TrySetResult();
        await open;

        Assert.Single(vm.Documents);
        Assert.Equal("controls", vm.Documents[0].RouteKey);
    }

    [WpfFact]
    public async Task Close_MovesSelection_ToNeighbor_And_Persists()
    {
        var vm = Create();
        await vm.OpenDocumentAsync("home");
        await vm.OpenDocumentAsync("controls");
        await vm.CloseDocumentAsync(vm.Documents[1]);

        Assert.Single(vm.Documents);
        Assert.Equal("home", vm.SelectedDocument?.RouteKey);

        var json = await File.ReadAllTextAsync(_settingsPath);
        Assert.Contains("\"home\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"controls\"", json, StringComparison.Ordinal);
    }

    [WpfFact]
    public void Move_ReordersTabs()
    {
        var vm = Create();
        vm.OpenDocumentAsync("home").GetAwaiter().GetResult();
        vm.OpenDocumentAsync("controls").GetAwaiter().GetResult();
        var controls = vm.Documents[1];

        vm.MoveSelectedRightCommand.Execute(null);
        Assert.Same(controls, vm.Documents[1]); // already last: no-op

        vm.SelectedDocument = vm.Documents[0];
        vm.MoveSelectedRightCommand.Execute(null);
        Assert.Equal("home", vm.Documents[1].RouteKey);

        vm.MoveSelectedLeftCommand.Execute(null);
        Assert.Equal("home", vm.Documents[0].RouteKey);
    }

    [WpfFact]
    public void History_BackForward_MovesSelection()
    {
        var vm = Create();
        vm.OpenDocumentAsync("home").GetAwaiter().GetResult();
        vm.OpenDocumentAsync("controls").GetAwaiter().GetResult();

        Assert.True(vm.CanGoBack);
        vm.GoBackCommand.Execute(null);
        Assert.Equal("home", vm.SelectedDocument?.RouteKey);
        Assert.True(vm.CanGoForward);
        vm.GoForwardCommand.Execute(null);
        Assert.Equal("controls", vm.SelectedDocument?.RouteKey);
    }

    [WpfFact]
    public async Task MalformedFile_RecoversToHome_WithDefaultWidth()
    {
        // Hand-edited/corrupt workspace state: unknown keys filtered,
        // out-of-range width reset, selection falls back — startup never fails.
        await File.WriteAllTextAsync(
            _settingsPath,
            """{"Workspace": {"OpenDocuments": ["home", "nope"], "SelectedDocument": "nope", "DetailWidth": -50, "IsDetailOpen": true}}""");
        var settings = new SettingsService(_settingsPath);
        await settings.LoadAsync();
        var vm = new WorkspaceViewModel(settings, _ => new Page());
        vm.EnsureLoaded();

        Assert.Single(vm.Documents);
        Assert.Equal("home", vm.SelectedDocument?.RouteKey);
        Assert.Equal(280, vm.DetailWidth);
    }

    [WpfFact]
    public void WorkspaceState_DefaultsToHomeDocument()
    {
        var state = new WorkspaceState();

        Assert.Equal(["home"], state.OpenDocuments);
        Assert.Equal(280, state.DetailWidth);
        Assert.True(state.IsDetailOpen);
    }

    [WpfFact]
    public async Task DetailWidth_RoundTrips_ThroughPersistence()
    {
        var vm = Create();
        await vm.OpenDocumentAsync("home");
        vm.DetailWidth = 350;
        await vm.PersistAsync();

        var settings = new SettingsService(_settingsPath);
        await settings.LoadAsync(TestContext.Current.CancellationToken);
        var reloaded = new WorkspaceViewModel(settings, _ => new Page());
        reloaded.EnsureLoaded();

        Assert.Equal(350, reloaded.DetailWidth);
        Assert.Equal("home", reloaded.SelectedDocument?.RouteKey);
    }
}
