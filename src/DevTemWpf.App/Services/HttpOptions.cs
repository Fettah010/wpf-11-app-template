namespace DevTemWpf.App.Services;

/// <summary>HTTP client options (bound from the "Http" configuration section).</summary>
public sealed record HttpOptions
{
    public const string SectionName = "Http";

    /// <summary>Empty = client disabled; no remote calls are made by default.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 10;
}
