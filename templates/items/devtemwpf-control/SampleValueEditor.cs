using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace DevTemWpf.App.Controls;

/// <summary>
/// Reusable numeric value editor control with increment/decrement buttons,
/// clamped value range, dependency properties, commands, and an automation peer.
/// </summary>
public class SampleValueEditor : Control
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(SampleValueEditor),
            new PropertyMetadata(0, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(int),
            typeof(SampleValueEditor),
            new PropertyMetadata(0));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(int),
            typeof(SampleValueEditor),
            new PropertyMetadata(100));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SampleValueEditor),
            new PropertyMetadata("Value"));

    static SampleValueEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SampleValueEditor),
            new FrameworkPropertyMetadata(typeof(SampleValueEditor)));
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }

    public SampleValueEditor()
    {
        IncrementCommand = new RelayCommand(Increment, CanIncrement);
        DecrementCommand = new RelayCommand(Decrement, CanDecrement);
    }

    private void Increment()
    {
        if (Value < Maximum)
        {
            Value++;
        }
    }

    private bool CanIncrement() => Value < Maximum;

    private void Decrement()
    {
        if (Value > Minimum)
        {
            Value--;
        }
    }

    private bool CanDecrement() => Value > Minimum;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // RelayCommand.CanExecute is only re-queried on CommandManager input events;
        // a dependency-property change has to ask for it explicitly.
        CommandManager.InvalidateRequerySuggested();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new SampleValueEditorAutomationPeer(this);
    }
}
