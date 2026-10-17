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

    /// <summary>
    /// Serializes settings IO (1.4.0): workspace selection changes fire
    /// overlapping persists, and concurrent writes to the same file throw
    /// IOException on Windows. Readers join the same gate so they never
    /// observe a torn write.
    /// </summary>
    private readonly SemaphoreSlim _ioGate = new(1, 1);

    private readonly string _filePath;

    public SettingsService(string? filePath = null)
    {
        _filePath = filePath ?? DefaultPath();
        Current = new ShellSettings();
    }

    public ShellSettings Current { get; private set; }

    public event EventHandler<ShellSettings>? Updated;

    public static string DefaultPath() => AppPaths.SettingsPath;

    /// <summary>
    /// Loads persisted settings. Returns true when the file was missing or
    /// corrupt (caller should persist to repair); false when loaded cleanly.
    /// </summary>
    public async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _ioGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath, cancellationToken).ConfigureAwait(false);
                Current = JsonSerializer.Deserialize<ShellSettings>(json, JsonOptions) ?? new ShellSettings();
                return false;
            }
            catch
            {
                // Missing, unreadable, or corrupt → safe defaults (resilience rule).
                Current = new ShellSettings();
                return true;
            }
        }
        finally
        {
            _ioGate.Release();
        }
    }

    public async Task UpdateAsync(Func<ShellSettings, ShellSettings> mutate, CancellationToken cancellationToken = default)
    {
        await _ioGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
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
        finally
        {
            _ioGate.Release();
        }
    }
}
