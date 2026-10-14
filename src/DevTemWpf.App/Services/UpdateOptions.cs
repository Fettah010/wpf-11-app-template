namespace DevTemWpf.App.Services;

/// <summary>Update channel (feed selection).</summary>
public enum UpdateChannel
{
    Stable = 0,
    Beta = 1,
}

/// <summary>Update feed options (bound from the "Update" configuration section).</summary>
public sealed record UpdateOptions
{
    public const string SectionName = "Update";

    /// <summary>Empty = updates disabled; the app never checks on its own.</summary>
    public string StableFeedUrl { get; init; } = string.Empty;

    public string BetaFeedUrl { get; init; } = string.Empty;
}

/// <summary>Update state machine states (user-visible).</summary>
public enum UpdateStatus
{
    Disabled = 0,
    Idle = 1,
    Checking = 2,
    Available = 3,
    UpToDate = 4,
    Downloading = 5,
    ReadyToRestart = 6,
    Failed = 7,
}
