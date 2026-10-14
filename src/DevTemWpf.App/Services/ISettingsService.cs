namespace DevTemWpf.App.Services;

/// <summary>Persisted shell preferences with change notification.</summary>
public interface ISettingsService
{
    ShellSettings Current { get; }

    event EventHandler<ShellSettings>? Updated;

    Task LoadAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(Func<ShellSettings, ShellSettings> mutate, CancellationToken cancellationToken = default);
}
