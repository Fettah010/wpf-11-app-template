using System.Windows.Controls;
using DevTemWpf.App.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DevTemWpf.Unit;

public sealed class ControlsViewModelTests
{
    private sealed class FakeSnackbar : ISnackbarService
    {
        public List<(string Title, string Message, ControlAppearance Appearance, TimeSpan Timeout)> Shown { get; } = [];

        public TimeSpan DefaultTimeOut { get; set; } = TimeSpan.FromSeconds(2);

        public void SetSnackbarPresenter(SnackbarPresenter contentPresenter) { }

        public SnackbarPresenter? GetSnackbarPresenter() => null;

        public void Show(string title, string message, ControlAppearance appearance, IconElement? icon, TimeSpan timeout)
            => Shown.Add((title, message, appearance, timeout));
    }

    private sealed class FakeDialogs : IContentDialogService
    {
        public ContentDialogResult Result { get; set; } = ContentDialogResult.Primary;
        public List<ContentDialog> Shown { get; } = [];

        public void SetDialogHost(ContentDialogHost host) { }

        public ContentDialogHost? GetDialogHostEx() => null;

#pragma warning disable CS0618 // IContentDialogService obsolete members: implement, never call.
        public void SetDialogHost(ContentPresenter contentPresenter) { }

        public ContentPresenter? GetDialogHost() => null;

        public void SetContentPresenter(ContentPresenter contentPresenter) { }

        public ContentPresenter? GetContentPresenter() => null;
#pragma warning restore CS0618

        public Task<ContentDialogResult> ShowAsync(ContentDialog dialog, CancellationToken cancellationToken)
        {
            Shown.Add(dialog);
            return Task.FromResult(Result);
        }
    }

    private static (ControlsViewModel Vm, FakeSnackbar Snackbar, FakeDialogs Dialogs) Create()
    {
        var snackbar = new FakeSnackbar();
        var dialogs = new FakeDialogs();
        return (new ControlsViewModel(snackbar, dialogs, new FormViewModel()), snackbar, dialogs);
    }

    [WpfFact]
    public void ShowSnackbar_PassesTitleMessageAndAppearance()
    {
        var (vm, snackbar, _) = Create();

        vm.ShowSnackbarCommand.Execute(null);

        var shown = Assert.Single(snackbar.Shown);
        Assert.Equal("Snackbar via service", shown.Title);
        Assert.Contains("ISnackbarService", shown.Message);
        Assert.Equal(ControlAppearance.Primary, shown.Appearance);
        Assert.Equal(TimeSpan.FromSeconds(3), shown.Timeout);
        Assert.Equal("Snackbar shown (3s).", vm.StatusText);
    }

    [WpfFact]
    public async Task ConfirmDialog_WritesResultToStatus()
    {
        var (vm, _, dialogs) = Create();
        dialogs.Result = ContentDialogResult.Primary;

        await vm.ShowConfirmDialogCommand.ExecuteAsync(null);

        Assert.Equal("Dialog result: Primary.", vm.StatusText);
    }

    [WpfFact]
    public async Task ConfirmDialog_CancelPath_WritesNone()
    {
        var (vm, _, dialogs) = Create();
        dialogs.Result = ContentDialogResult.None;

        await vm.ShowConfirmDialogCommand.ExecuteAsync(null);

        Assert.Equal("Dialog result: None.", vm.StatusText);
    }

    [Fact]
    public async Task BusyDemo_DisablesDuringWork_And_ReentrancyIsIgnored()
    {
        var (vm, _, _) = Create();

        var first = vm.RunBusyDemoCommand.ExecuteAsync(null);
        Assert.True(vm.IsBusy);
        Assert.False(vm.IsIdle);

        await vm.RunBusyDemoCommand.ExecuteAsync(null);
        await first;

        Assert.False(vm.IsBusy);
        Assert.True(vm.IsIdle);
        Assert.Equal(DevTemWpf.App.Resources.Strings.Ctl_StatusDone, vm.StatusText);
    }

    [Fact]
    public async Task BusyDemo_Cancel_ReportsCancelledAndIdles()
    {
        var vm = new ControlsViewModel(new FakeSnackbar(), new FakeDialogs(), new FormViewModel());

        var running = vm.RunBusyDemoCommand.ExecuteAsync(null);
        Assert.True(vm.IsBusy);
        Assert.True(vm.CancelBusyDemoCommand.CanExecute(null));
        vm.CancelBusyDemoCommand.Execute(null);
        await running;

        Assert.False(vm.IsBusy);
        Assert.True(vm.IsIdle);
        Assert.Equal(DevTemWpf.App.Resources.Strings.Ctl_StatusCancelled, vm.StatusText);
    }
}
