using System.IO;
using System.Text.Json;

namespace DevTemWpf.App.Services;

/// <summary>
/// JSON-file settings. Corrupt/missing files fall back to defaults and are
/// repaired on the next save — startup never fails because of settings.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public SettingsService(string? filePath = null)
    {
        _filePath = filePath ?? DefaultPath();
        Current = new ShellSettings();
    }

    public ShellSettings Current { get; private set; }

    public event EventHandler<ShellSettings>? Updated;

    public static string DefaultPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Wpf11AppTemplate");
        return Path.Combine(dir, "settings.json");
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken).ConfigureAwait(false);
            Current = JsonSerializer.Deserialize<ShellSettings>(json, JsonOptions) ?? new ShellSettings();
        }
        catch
        {
            // Missing, unreadable, or corrupt → safe defaults (resilience rule).
            Current = new ShellSettings();
        }
    }

    public async Task UpdateAsync(Func<ShellSettings, ShellSettings> mutate, CancellationToken cancellationToken = default)
    {
        Current = mutate(Current);
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(Current, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken).ConfigureAwait(false);
        Updated?.Invoke(this, Current);
    }
}
