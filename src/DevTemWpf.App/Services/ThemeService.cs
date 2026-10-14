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

        ApplicationTheme theme = current.Theme switch
        {
            ThemeMode.Light => ApplicationTheme.Light,
            ThemeMode.Dark => ApplicationTheme.Dark,
            // System mode resolves explicitly so the persisted backdrop
            // choice is honored (ApplySystemTheme would force Mica).
            _ => _applier.GetSystemApplicationTheme(),
        };

        _applier.Apply(theme, effective);
        // Accent after theme: Apply resets accents to the OS color.
        _applier.ApplyAccentOverride(current.AccentOverride, theme);

        return Task.CompletedTask;
    }
}
