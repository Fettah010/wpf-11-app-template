using System.Windows.Controls;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Settings page (view wiring only).</summary>
public partial class SettingsPage : Page, INavigableView<SettingsViewModel>
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }
}
