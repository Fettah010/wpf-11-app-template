using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Home page ViewModel (placeholder content until 0.6.0 dashboard).</summary>
public sealed partial class HomeViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private string headline = "Welcome to WPF 11 App Template";

    [ObservableProperty]
    private string subline = "Rail/sidebar shell, theme switching, and settings persistence are live in 0.2.0.";

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
