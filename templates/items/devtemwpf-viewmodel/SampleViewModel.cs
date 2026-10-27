using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Standalone ViewModel with observable properties, RelayCommand commands,
/// and navigation lifecycle hooks following project conventions.
/// </summary>
public sealed partial class SampleViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private void Increment()
    {
        Count++;
    }

    [RelayCommand]
    private void Decrement()
    {
        Count--;
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(100);
            Count = 0;
            Title = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
