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
                UpdateChannel = UpdateChannel.Beta,
                MinimizeToTray = true,
                Culture = "de",
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
        Assert.Equal(MotionMode.Reduced, reloaded.Current.Motion);
        Assert.False(reloaded.Current.EffectsEnabled);
    }

    [Fact]
    public async Task RoundTrip_PreservesEffectSwitches()
    {
        var service = new SettingsService(_path);
        await service.LoadAsync(TestContext.Current.CancellationToken);

        await service.UpdateAsync(
            _ => new ShellSettings
            {
                RippleEffectEnabled = false,
                GradientEffectEnabled = true,
                WarpEffectEnabled = true,
            },
            TestContext.Current.CancellationToken);

        var reloaded = new SettingsService(_path);
        await reloaded.LoadAsync(TestContext.Current.CancellationToken);

        Assert.False(reloaded.Current.RippleEffectEnabled);
        Assert.True(reloaded.Current.GradientEffectEnabled);
        Assert.True(reloaded.Current.WarpEffectEnabled);
    }

    [Fact]
    public async Task Load_LegacyMotionIntegers_MapToExpectedModes()
    {
        // 1.4.x persisted Full as integer 1 and Auto as 0; 1.5.0 appends
        // Balanced as 4. Old files must keep resolving Rich (Full).
        await File.WriteAllTextAsync(_path, "{\"Motion\":1}", TestContext.Current.CancellationToken);
        var rich = new SettingsService(_path);
        await rich.LoadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(MotionMode.Full, rich.Current.Motion);

        await File.WriteAllTextAsync(_path, "{\"Motion\":4}", TestContext.Current.CancellationToken);
        var balanced = new SettingsService(_path);
        await balanced.LoadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(MotionMode.Balanced, balanced.Current.Motion);
    }

    [Fact]
    public async Task LoadAsync_ReportsWhetherPersistIsNeeded()
    {
        var missing = new SettingsService(_path);
        Assert.True(await missing.LoadAsync(TestContext.Current.CancellationToken));

        var present = new SettingsService(_path);
        await present.UpdateAsync(static s => s, TestContext.Current.CancellationToken);
        Assert.False(await present.LoadAsync(TestContext.Current.CancellationToken));
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
