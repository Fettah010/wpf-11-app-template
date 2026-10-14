namespace DevTemWpf.App;

/// <summary>Typed application options bound from the "App" configuration section.</summary>
public sealed record AppOptions
{
    public const string SectionName = "App";

    public string Title { get; init; } = "DevTemWpf";
    public double Width { get; init; } = 800;
    public double Height { get; init; } = 600;
}
