using System.Windows.Controls;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Controls gallery page (view wiring only).</summary>
public partial class ControlsPage : Page, INavigableView<ControlsViewModel>
{
    public ControlsPage(ControlsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public ControlsViewModel ViewModel { get; }
}
