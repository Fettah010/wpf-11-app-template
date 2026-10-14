using System.Windows.Controls;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Home page (view wiring only).</summary>
public partial class HomePage : Page, INavigableView<HomeViewModel>
{
    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public HomeViewModel ViewModel { get; }
}
