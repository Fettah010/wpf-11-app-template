using System.Windows;
using System.Windows.Controls;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Microsoft.Win32;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>
/// Diagnostics page. File pickers live here (OS chrome, view boundary);
/// the ViewModel takes explicit paths and stays testable.
/// </summary>
public partial class DiagnosticsPage : Page, INavigableView<DiagnosticsViewModel>
{
    public DiagnosticsPage(DiagnosticsViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public DiagnosticsViewModel ViewModel { get; }

    private void OnExportLogClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"app-log-{DateTime.Now:yyyyMMdd}.log",
            Filter = "Log files (*.log)|*.log|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog() == true)
        {
            ViewModel.ExportLog(dialog.FileName);
        }
    }

    private void OnBundleClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"support-bundle-{DateTime.Now:yyyyMMdd-HHmmss}.zip",
            Filter = "Zip files (*.zip)|*.zip",
        };
        if (dialog.ShowDialog() == true)
        {
            ViewModel.CreateSupportBundle(dialog.FileName);
        }
    }

    private void OnRestoreClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Backup files (*.zip)|*.zip" };
        if (dialog.ShowDialog() == true)
        {
            ViewModel.RestoreBackup(dialog.FileName);
        }
    }
}
