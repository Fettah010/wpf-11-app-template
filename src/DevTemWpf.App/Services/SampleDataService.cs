namespace DevTemWpf.App.Services;

/// <summary>Deterministic sample data for the dashboard (no backend in 0.6.0).</summary>
public sealed record ActivityItem(int Id, DateTime Timestamp, string Name, string Category, string Status, double Value);

/// <summary>Stat card content.</summary>
public sealed record MetricItem(string Label, string Value, string Delta, bool DeltaUp);

/// <summary>
/// Seeded (`Random(42)`) sample provider. Same process, same data — tests
/// assert determinism and counts.
/// </summary>
public sealed class SampleDataService
{
    private static readonly string[] Names =
    [
        "Sync worker", "Backup job", "Index builder", "Telemetry flush", "Cache warmer",
        "Health probe", "Log rotator", "Snapshot writer", "Policy evaluator", "Queue drainer",
    ];

    private static readonly string[] Categories = ["System", "Network", "Storage", "Compute"];

    private static readonly string[] Statuses = ["Success", "Success", "Success", "Info", "Warning", "Error"];

    public IReadOnlyList<MetricItem> GetMetrics() =>
    [
        new("Active tasks", "1,284", "+4.2% vs yesterday", true),
        new("Success rate", "98.6%", "+0.3 pts", true),
        new("Median latency", "41 ms", "-6 ms", true),
        new("Queue depth", "17", "+3", false),
    ];

    public IReadOnlyList<ActivityItem> GetActivities(int count = 1500)
    {
        var rng = new Random(42);
        var start = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Local);
        var items = new List<ActivityItem>(count);
        for (var i = 0; i < count; i++)
        {
            items.Add(new ActivityItem(
                i + 1,
                start.AddMinutes(-i * 2),
                Names[rng.Next(Names.Length)],
                Categories[rng.Next(Categories.Length)],
                Statuses[rng.Next(Statuses.Length)],
                Math.Round(rng.NextDouble() * 100, 1)));
        }

        return items;
    }

    public IReadOnlyList<string> GetWeekdayLabels() => ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public IReadOnlyList<double> GetWeeklyValues() => [42, 68, 55, 81, 63, 29, 74];
}
