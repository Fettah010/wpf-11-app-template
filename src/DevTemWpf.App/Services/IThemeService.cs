namespace DevTemWpf.App.Services;

/// <summary>Applies the persisted theme preference at runtime.</summary>
public interface IThemeService
{
    Task ApplyCurrentAsync(CancellationToken cancellationToken = default);
}
