using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Resources;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Controls gallery ViewModel. Talks to feedback <i>services</i>, never to
/// controls — every command is unit-testable via fakes (ADR-0008).
/// </summary>
public sealed partial class ControlsViewModel : ObservableObject
{
    private readonly ISnackbarService _snackbar;
    private readonly IContentDialogService _dialogs;

    public ControlsViewModel(ISnackbarService snackbar, IContentDialogService dialogs, FormViewModel form)
    {
        _snackbar = snackbar;
        _dialogs = dialogs;
        Form = form;
    }

    public FormViewModel Form { get; }

    [ObservableProperty]
    private string statusText = Strings.Ctl_StatusPick;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    private bool isBusy;

    public bool IsIdle => !IsBusy;

    [RelayCommand]
    private void ShowSnackbar()
    {
        _snackbar.Show(
            "Snackbar via service",
            "Title, message, appearance, and timeout travel through ISnackbarService.",
            ControlAppearance.Primary,
            new SymbolIcon(SymbolRegular.Checkmark24),
            TimeSpan.FromSeconds(3));
        StatusText = Strings.Ctl_StatusSnackbar;
    }

    [RelayCommand]
    private async Task ShowConfirmDialogAsync(CancellationToken cancellationToken)
    {
        var result = await _dialogs.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions
            {
                Title = "Delete sample item?",
                Content = "This demonstrates the confirmation round-trip. Nothing is deleted.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
            },
            cancellationToken).ConfigureAwait(true);
        StatusText = string.Format(Strings.Ctl_StatusDialog, result);
    }

    // Single-flight declaratively: concurrent invocations are ignored, so the
    // IsBusy guard below only ever observes its own execution.
    // NOTE: no CancellationToken parameter — the command-supplied token was
    // observed pre-cancelled under the MTP/WpfFact runner (see 0.4.0 report);
    // cancellation is a documented non-goal for this demo.
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task RunBusyDemoAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusText = Strings.Ctl_StatusWorking;
            await Task.Delay(TimeSpan.FromMilliseconds(900)).ConfigureAwait(true);
            StatusText = Strings.Ctl_StatusDone;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
