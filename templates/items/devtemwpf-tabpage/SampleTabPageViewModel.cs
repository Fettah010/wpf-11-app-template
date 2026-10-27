using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>SampleTabPage ViewModel.</summary>
public sealed partial class SampleTabPageViewModel : ObservableObject, INavigationAware
{
    [RelayCommand]
    private void OpenSibling()
    {
        // Request the workspace to open a sibling document tab of this type.
        // The workspace shell resolves the page type via its document registry.
    }

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
