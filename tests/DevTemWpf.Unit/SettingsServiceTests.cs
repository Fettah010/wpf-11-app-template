using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    [Fact]
    public async Task RoundTrip_PreservesValues()
    {
        var service = new SettingsService(_path);
        await service.LoadAsync(TestContext.Current.CancellationToken);

        await service.UpdateAsync(
            _ => new ShellSettings
            {
                Theme = ThemeMode.Dark,
                BackdropEnabled = false,
                Shell = ShellMode.Rail,
                Density = DensityMode.Compact,
                AccentOverride = "#4CC2FF",
                Motion = MotionMode.Reduced,
                EffectsEnabled = false,
            },
            TestContext.Current.CancellationToken);

        var reloaded = new SettingsService(_path);
        await reloaded.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(ThemeMode.Dark, reloaded.Current.Theme);
        Assert.False(reloaded.Current.BackdropEnabled);
        Assert.Equal(ShellMode.Rail, reloaded.Current.Shell);
        Assert.Equal(DensityMode.Compact, reloaded.Current.Density);
        Assert.Equal("#4CC2FF", reloaded.Current.AccentOverride);
        Assert.Equal(MotionMode.Reduced, reloaded.Current.Motion);
        Assert.False(reloaded.Current.EffectsEnabled);
    }

    [Fact]
    public async Task MissingFile_YieldsDefaults()
    {
        var service = new SettingsService(_path);

        await service.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new ShellSettings(), service.Current);
    }

    [Fact]
    public async Task CorruptFile_YieldsDefaults()
    {
        await File.WriteAllTextAsync(_path, "{ not json", TestContext.Current.CancellationToken);
        var service = new SettingsService(_path);

        await service.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new ShellSettings(), service.Current);
    }

    [Fact]
    public async Task Update_RaisesUpdated_WithNewValue()
    {
        var service = new SettingsService(_path);
        ShellSettings? seen = null;
        service.Updated += (_, s) => seen = s;

        await service.UpdateAsync(s => s with { Theme = ThemeMode.Light }, TestContext.Current.CancellationToken);

        Assert.Equal(ThemeMode.Light, seen?.Theme);
    }
}
