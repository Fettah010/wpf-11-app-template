using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.Views;

/// <summary>Controls gallery page (view wiring only).</summary>
public partial class ControlsPage : Page, INavigableView<ControlsViewModel>
{
    public ControlsPage(ControlsViewModel viewModel, IMotionService motion)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += (_, _) => _ = motion.PlayEntranceAsync(this);
    }

    public ControlsViewModel ViewModel { get; }

    /// <summary>
    /// Category index: scrolls the section heading into view and moves
    /// keyboard focus to it (button activation already covers mouse,
    /// keyboard, and UI automation).
    /// </summary>
    private void OnIndexClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string targetName })
        {
            return;
        }

        if (FindName(targetName) is not FrameworkElement target)
        {
            return;
        }

        var offset = target.TransformToVisual(GalleryScroller).Transform(new Point(0, 0));
        GalleryScroller.ScrollToVerticalOffset(GalleryScroller.VerticalOffset + offset.Y);
        target.Focus();
    }
}
