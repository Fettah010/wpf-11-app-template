using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class FavoritesStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
    }

    private FavoritesStore Create() => new(Path.Combine(_dir, "app.db"));

    [Fact]
    public void Toggle_AddsAndRemoves()
    {
        var store = Create();

        Assert.True(store.ToggleFavorite(7));
        Assert.True(store.IsFavorite(7));
        Assert.False(store.ToggleFavorite(7));
        Assert.False(store.IsFavorite(7));
    }

    [Fact]
    public void Favorites_PersistAcrossInstances()
    {
        Create().ToggleFavorite(42);

        var reopened = Create();

        Assert.True(reopened.IsFavorite(42));
        Assert.Equal([42], reopened.GetFavorites());
    }

    [Fact]
    public void FreshStore_IsEmpty()
    {
        Assert.Empty(Create().GetFavorites());
    }
}
