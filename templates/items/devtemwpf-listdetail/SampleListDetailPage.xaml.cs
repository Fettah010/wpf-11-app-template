using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>SampleListDetailPage code-behind.</summary>
public partial class SampleListDetailPage : Page, INavigableView<SampleListDetailPageViewModel>
{
    public SampleListDetailPage(SampleListDetailPageViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public SampleListDetailPageViewModel ViewModel { get; }
}
