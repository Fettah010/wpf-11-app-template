using Wpf.Ui.Appearance;

namespace DevTemWpf.App.Services;

/// <summary>
/// Maps <see cref="ThemeMode"/> to WPF-UI and applies it together with the
/// effective backdrop. Window chrome itself is set in code-behind (view
/// boundary); this service owns the mapping (ADR-0006).
/// </summary>
public sealed class ThemeService : IThemeService
{
    private readonly ISettingsService _settings;
    private readonly IBackdropService _backdrop;
    private readonly IThemeApplier _applier;

    public ThemeService(ISettingsService settings, IBackdropService backdrop, IThemeApplier applier)
    {
        _settings = settings;
        _backdrop = backdrop;
        _applier = applier;
    }

    public Task ApplyCurrentAsync(CancellationToken cancellationToken = default)
    {
        var current = _settings.Current;
        var effective = _backdrop.Current(current.BackdropEnabled);

        switch (current.Theme)
        {
            case ThemeMode.Light:
                _applier.Apply(ApplicationTheme.Light, effective);
                break;
            case ThemeMode.Dark:
                _applier.Apply(ApplicationTheme.Dark, effective);
                break;
            default:
                _applier.ApplySystemTheme();
                break;
        }

        return Task.CompletedTask;
    }
}
