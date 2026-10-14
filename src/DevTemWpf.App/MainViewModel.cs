using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;

namespace DevTemWpf.App;

/// <summary>Minimal ViewModel proving the MVVM source-generator path.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly AppOptions _options;
    private int _pressCount;

    public MainViewModel(IOptions<AppOptions> options)
    {
        _options = options.Value;
        StatusText = $"Ready — {_options.Title} (press the button)";
    }

    [ObservableProperty]
    private string statusText = string.Empty;

    [RelayCommand]
    private Task GreetAsync(CancellationToken cancellationToken)
    {
        _pressCount++;
        StatusText = $"Hello from {_options.Title} — pressed {_pressCount}x";
        return Task.CompletedTask;
    }
}
