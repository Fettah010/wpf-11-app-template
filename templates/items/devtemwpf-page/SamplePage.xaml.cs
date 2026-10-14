using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>
/// SamplePage view wiring. After scaffolding, register in AppComposition
/// (AddTransient for page + ViewModel) and add a MainViewModel nav item:
/// <code>new NavigationViewItem("SamplePage", SymbolRegular.DocumentAdd24, typeof(SamplePage)),</code>
/// </summary>
public partial class SamplePage : Page, INavigableView<SamplePageViewModel>
{
    public SamplePage(SamplePageViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public SamplePageViewModel ViewModel { get; }
}
