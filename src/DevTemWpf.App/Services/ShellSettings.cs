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
{
    /// <summary>Compact icon rail (<c>LeftCompact</c>).</summary>
    Rail = 0,
    /// <summary>Full expandable sidebar (<c>Left</c>).</summary>
    Sidebar = 1,
}

/// <summary>Persisted shell preferences (DataContract-free JSON record).</summary>
public sealed record ShellSettings
{
    public ThemeMode Theme { get; init; } = ThemeMode.System;
    public bool BackdropEnabled { get; init; } = true;
    public ShellMode Shell { get; init; } = ShellMode.Sidebar;
}
