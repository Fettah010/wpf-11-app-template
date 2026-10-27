using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>SampleDataGridPage code-behind.</summary>
public partial class SampleDataGridPage : Page, INavigableView<SampleDataGridPageViewModel>
{
    public SampleDataGridPage(SampleDataGridPageViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public SampleDataGridPageViewModel ViewModel { get; }
}
