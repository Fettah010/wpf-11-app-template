using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>
/// SampleTabPage view wiring. Designed for the Tabbed shell document manager;
/// register in AppComposition and add to the workspace's document registry.
/// </summary>
public partial class SampleTabPage : Page, INavigableView<SampleTabPageViewModel>
{
    public SampleTabPage(SampleTabPageViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public SampleTabPageViewModel ViewModel { get; }
}
