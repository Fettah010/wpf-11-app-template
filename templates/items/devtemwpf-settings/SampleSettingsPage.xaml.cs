using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>
/// SampleSettingsPage view wiring. Register in AppComposition (AddTransient)
/// and add to navigation if needed.
/// </summary>
public partial class SampleSettingsPage : Page, INavigableView<SampleSettingsPageViewModel>
{
    public SampleSettingsPage(SampleSettingsPageViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public SampleSettingsPageViewModel ViewModel { get; }
}
