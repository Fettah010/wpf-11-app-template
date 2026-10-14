using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Home page ViewModel.</summary>
public sealed partial class HomeViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private string headline = "Welcome to WPF 11 App Template";

    [ObservableProperty]
    private string subline = "Rail/sidebar shell, theme switching, and settings persistence.";

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
