namespace DevTemWpf.App.Services;

/// <summary>Application theme preference.</summary>
public enum ThemeMode
{
    System = 0,
    Light = 1,
    Dark = 2,
}

/// <summary>Shell layout preference (ADR-0005).</summary>
public enum ShellMode
{    /// <summary>Compact icon rail (<c>LeftMinimal</c>).</summary>
    Rail = 0,
    /// <summary>Full expandable sidebar (<c>Left</c>).</summary>
    Sidebar = 1,
}

/// <summary>Spacing density preference (ADR-0007).</summary>
public enum DensityMode
{
    Comfortable = 0,
    Compact = 1,
}

/// <summary>Update channel (feed selection). Lives here so ShellSettings
/// never depends on the excludable UpdateOptions module.</summary>
public enum UpdateChannel
{
    Stable = 0,
    Beta = 1,
}

/// <summary>Persisted shell preferences (DataContract-free JSON record).</summary>
public sealed record ShellSettings
{
    public ThemeMode Theme { get; init; } = ThemeMode.System;
    public bool BackdropEnabled { get; init; } = true;
    public ShellMode Shell { get; init; } = ShellMode.Sidebar;
    public DensityMode Density { get; init; } = DensityMode.Comfortable;

    /// <summary>Custom accent as <c>#RRGGBB</c> or <c>#AARRGGBB</c>; null = system accent.</summary>
    public string? AccentOverride { get; init; }

    public MotionMode Motion { get; init; } = MotionMode.Auto;

    /// <summary>Master switch for premium effects (ripple, animated surfaces).</summary>
    public bool EffectsEnabled { get; init; } = true;

    public UpdateChannel UpdateChannel { get; init; } = UpdateChannel.Stable;

    public bool MinimizeToTray { get; init; }

    /// <summary>UI culture name (e.g. "de"); empty = system culture.</summary>
    public string Culture { get; init; } = string.Empty;
}
