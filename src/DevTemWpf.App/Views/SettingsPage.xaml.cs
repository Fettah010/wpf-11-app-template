using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Settings page (view wiring only).</summary>
public partial class SettingsPage : Page, INavigableView<SettingsViewModel>
{
    public SettingsPage(SettingsViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public SettingsViewModel ViewModel { get; }

    private void OnRestartClick(object sender, RoutedEventArgs e)
    {
        var exe = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(exe))
        {
            _ = Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
        }

        Application.Current.Shutdown();
    }
}
