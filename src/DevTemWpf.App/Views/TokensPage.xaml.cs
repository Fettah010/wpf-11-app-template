using System.Windows.Controls;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Token preview page (view wiring only).</summary>
public partial class TokensPage : Page, INavigableView<TokensViewModel>
{
    public TokensPage(TokensViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public TokensViewModel ViewModel { get; }
}
