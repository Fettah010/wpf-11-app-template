using System.Windows;
using Microsoft.Extensions.Options;

namespace DevTemWpf.App;

/// <summary>
/// View wiring only: resolves the ViewModel via DI, applies configured
/// window chrome values. No business logic here.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, IOptions<AppOptions> options)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;
        Title = options.Value.Title;
        Width = options.Value.Width;
        Height = options.Value.Height;
    }

    public MainViewModel ViewModel { get; }
}
