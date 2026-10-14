using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>Token preview page ViewModel (disposable example until 0.6.0).</summary>
public sealed partial class TokensViewModel : ObservableObject, INavigationAware
{
    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
