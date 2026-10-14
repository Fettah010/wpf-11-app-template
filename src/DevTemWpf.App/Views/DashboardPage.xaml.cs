using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Dashboard page (view wiring only).</summary>
public partial class DashboardPage : Page, INavigableView<DashboardViewModel>
{
    public DashboardPage(DashboardViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public DashboardViewModel ViewModel { get; }
}
