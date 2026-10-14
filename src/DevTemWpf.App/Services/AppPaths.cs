using System.IO;

namespace DevTemWpf.App.Services;

/// <summary>Well-known user-data locations (ADR-0011).</summary>
public static class AppPaths
{
    public static string BaseDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Wpf11AppTemplate");

    public static string SettingsPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Wpf11AppTemplate", "settings.json");

    public static string LogsDir { get; } = Path.Combine(BaseDir, "logs");

    public static string DbPath { get; } = Path.Combine(BaseDir, "app.db");

    public static string BackupsDir { get; } = Path.Combine(BaseDir, "backups");
}
