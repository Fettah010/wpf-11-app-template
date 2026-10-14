namespace DevTemWpf.App.Services;

/// <summary>Persisted shell preferences with change notification.</summary>
public interface ISettingsService
{
    ShellSettings Current { get; }

    event EventHandler<ShellSettings>? Updated;

    /// <summary>
    /// Loads persisted settings. Returns true when the file was missing or
    /// corrupt (caller should persist to repair); false when loaded cleanly.
    /// </summary>
    Task<bool> LoadAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(Func<ShellSettings, ShellSettings> mutate, CancellationToken cancellationToken = default);
}
