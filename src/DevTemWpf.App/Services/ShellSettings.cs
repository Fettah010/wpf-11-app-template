namespace DevTemWpf.App.Services;

/// <summary>Application theme preference.</summary>
public enum ThemeMode
{
    System = 0,
    Light = 1,
    Dark = 2,
}

/// <summary>Shell layout preference (ADR-0005; Top/Tabbed added in 1.4.0).</summary>
public enum ShellMode
{    /// <summary>Compact icon rail (<c>LeftMinimal</c>).</summary>
    Rail = 0,
    /// <summary>Full expandable sidebar (<c>Left</c>).</summary>
    Sidebar = 1,
    /// <summary>Horizontal top navigation bar (<c>Top</c>).</summary>
    Top = 2,
    /// <summary>Document-workspace shell: rail chrome plus tabbed documents.</summary>
    Tabbed = 3,
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

/// <summary>Persisted tabbed-workspace state (1.4.0).</summary>
public sealed record WorkspaceState : IEquatable<WorkspaceState>
{
    /// <summary>Open document route keys, in tab order.</summary>
    public string[] OpenDocuments { get; init; } = ["home"];

    /// <summary>Selected document route key (falls back to first open).</summary>
    public string? SelectedDocument { get; init; }

    /// <summary>Detail-pane width in device-independent pixels.</summary>
    public double DetailWidth { get; init; } = 280;

    /// <summary>Whether the detail pane is expanded.</summary>
    public bool IsDetailOpen { get; init; } = true;

    /// <summary>
    /// Value equality: records compare array members by reference, but an
    /// empty-vs-default round-trip must stay equal (the settings
    /// missing/corrupt-file tests assert record equality). Empty collection
    /// expressions already share the Array.Empty singleton; non-empty
    /// document lists compare by sequence here.
    /// </summary>
    public bool Equals(WorkspaceState? other)
        => other is not null
            && OpenDocuments.SequenceEqual(other.OpenDocuments)
            && SelectedDocument == other.SelectedDocument
            && DetailWidth.Equals(other.DetailWidth)
            && IsDetailOpen == other.IsDetailOpen;

    public override int GetHashCode()
        => HashCode.Combine(
            OpenDocuments.Aggregate(0, HashCode.Combine),
            SelectedDocument,
            DetailWidth,
            IsDetailOpen);
}

/// <summary>Persisted shell preferences (DataContract-free JSON record).</summary>
public sealed record ShellSettings
{
    public ThemeMode Theme { get; init; } = ThemeMode.System;
    public bool BackdropEnabled { get; init; } = true;
    public ShellMode Shell { get; init; } = ParseShellDefault();
    public DensityMode Density { get; init; } = DensityMode.Comfortable;

    /// <summary>Custom accent as <c>#RRGGBB</c> or <c>#AARRGGBB</c>; null = system accent.</summary>
    public string? AccentOverride { get; init; }

    public MotionMode Motion { get; init; } = MotionMode.Auto;

    /// <summary>Master switch for premium effects (ripple, animated surfaces).</summary>
    public bool EffectsEnabled { get; init; } = true;

    public UpdateChannel UpdateChannel { get; init; } = UpdateChannel.Stable;

    /// <summary>Expanded data-tree node IDs (1.3.0 sample persistence).</summary>
    public string[] ExpandedDataNodes { get; init; } = [];

    /// <summary>Tabbed-shell workspace state (1.4.0, additive).</summary>
    public WorkspaceState Workspace { get; init; } = new();

    public bool MinimizeToTray { get; init; }

    /// <summary>UI culture name (e.g. "de"); empty = system culture.</summary>
    public string Culture { get; init; } = string.Empty;

    /// <summary>
    /// Scaffold-time default shell (1.4.0 T08). The template engine replaces
    /// the <c>DevTemWpfShellDefault</c> token with the <c>--Shell</c> choice
    /// (Sidebar/Rail/Top/Tabbed); the token occurs nowhere else, so no other
    /// logic or test seed is affected. An unparseable token falls back to
    /// Sidebar, which is exactly the repository state (the token is never
    /// replaced in-repo), so the repo always builds and runs Sidebar-first.
    /// </summary>
    private static ShellMode ParseShellDefault()
        => Enum.TryParse<ShellMode>("DevTemWpfShellDefault", out var shell)
            ? shell
            : ShellMode.Sidebar;
}
