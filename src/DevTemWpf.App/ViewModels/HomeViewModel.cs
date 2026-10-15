using CommunityToolkit.Mvvm.ComponentModel;
using DevTemWpf.App.Resources;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Home page ViewModel.</summary>
public sealed partial class HomeViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private string headline = Strings.Home_Title;

    [ObservableProperty]
    private string subline = Strings.Home_Subtitle;

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
