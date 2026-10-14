using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Controls gallery page (view wiring only).</summary>
public partial class ControlsPage : Page, INavigableView<ControlsViewModel>
{
    public ControlsPage(ControlsViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public ControlsViewModel ViewModel { get; }
}
