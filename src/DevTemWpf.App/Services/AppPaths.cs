using System.IO;

namespace DevTemWpf.App.Services;

/// <summary>Well-known user-data locations (ADR-0011).</summary>
public static class AppPaths
{
    private static readonly string? TestDataRoot = Environment.GetEnvironmentVariable("DEVTEM_TEST_DATA_ROOT");

    public static string BaseDir { get; } = Path.Combine(
        GetDataDirectory(Environment.SpecialFolder.LocalApplicationData), "Wpf11AppTemplate");

    public static string SettingsPath { get; } = Path.Combine(
        GetDataDirectory(Environment.SpecialFolder.ApplicationData), "Wpf11AppTemplate", "settings.json");

    public static string LogsDir { get; } = Path.Combine(BaseDir, "logs");

    public static string DbPath { get; } = Path.Combine(BaseDir, "app.db");

    public static string BackupsDir { get; } = Path.Combine(BaseDir, "backups");

    //#if (IncludeCrash)
    public static string CrashDir { get; } = Path.Combine(BaseDir, "crashes");
    //#endif

    private static string GetDataDirectory(Environment.SpecialFolder folder)
    {
        if (!string.IsNullOrWhiteSpace(TestDataRoot))
        {
            var profileDirectory = folder == Environment.SpecialFolder.LocalApplicationData ? "Local" : "Roaming";
            return Path.Combine(TestDataRoot, profileDirectory);
        }

        return Environment.GetFolderPath(folder);
    }
}
