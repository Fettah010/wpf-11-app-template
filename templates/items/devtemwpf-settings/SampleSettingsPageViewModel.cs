using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>SampleSettingsPage ViewModel.</summary>
public sealed partial class SampleSettingsPageViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private bool _featureEnabled = true;

    [RelayCommand]
    private void ResetOptions()
    {
        FeatureEnabled = true;
    }

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
