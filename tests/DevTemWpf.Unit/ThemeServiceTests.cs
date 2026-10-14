using DevTemWpf.App.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DevTemWpf.Unit;

public sealed class ThemeServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    private sealed class FakeApplier : IThemeApplier
    {
        public List<(ApplicationTheme Theme, WindowBackdropType Backdrop)> Applied { get; } = [];
        public List<(string? Hex, ApplicationTheme Theme)> Accents { get; } = [];

        public void Apply(ApplicationTheme theme, WindowBackdropType backdrop) => Applied.Add((theme, backdrop));
        public ApplicationTheme GetSystemApplicationTheme() => ApplicationTheme.Dark;
        public void ApplyAccentOverride(string? hex, ApplicationTheme theme) => Accents.Add((hex, theme));
        public bool IsHighContrast() => false;
    }

    private sealed class FakeBackdrop : IBackdropService
    {
        public WindowBackdropType ResolveEffective(bool enabled, bool isHighContrast, bool isMicaSupported)
            => enabled ? WindowBackdropType.Mica : WindowBackdropType.None;

        public WindowBackdropType Current(bool enabled)
            => enabled ? WindowBackdropType.Mica : WindowBackdropType.None;
    }

    private async Task<(ThemeService Service, FakeApplier Applier)> CreateAsync(ShellSettings seed)
    {
        var settings = new SettingsService(_path);
        await settings.UpdateAsync(_ => seed, TestContext.Current.CancellationToken);
        var applier = new FakeApplier();
        return (new ThemeService(settings, new FakeBackdrop(), applier), applier);
    }

    [Fact]
    public async Task Light_AppliesLight_WithEffectiveBackdrop()
    {
        var (service, applier) = await CreateAsync(new ShellSettings { Theme = ThemeMode.Light });

        await service.ApplyCurrentAsync(TestContext.Current.CancellationToken);

        var call = Assert.Single(applier.Applied);
        Assert.Equal(ApplicationTheme.Light, call.Theme);
        Assert.Equal(WindowBackdropType.Mica, call.Backdrop);
    }

    [Fact]
    public async Task Dark_WithBackdropOff_AppliesNone()
    {
        var (service, applier) = await CreateAsync(new ShellSettings { Theme = ThemeMode.Dark, BackdropEnabled = false });

        await service.ApplyCurrentAsync(TestContext.Current.CancellationToken);

        var call = Assert.Single(applier.Applied);
        Assert.Equal(ApplicationTheme.Dark, call.Theme);
        Assert.Equal(WindowBackdropType.None, call.Backdrop);
    }

    [Fact]
    public async Task System_AppliesOsTheme_WithEffectiveBackdrop()
    {
        var (service, applier) = await CreateAsync(new ShellSettings { Theme = ThemeMode.System });

        await service.ApplyCurrentAsync(TestContext.Current.CancellationToken);

        var call = Assert.Single(applier.Applied);
        Assert.Equal(ApplicationTheme.Dark, call.Theme);
        Assert.Equal(WindowBackdropType.Mica, call.Backdrop);
    }

    [Fact]
    public async Task AccentOverride_AppliedAfterTheme_WithResolvedTheme()
    {
        var (service, applier) = await CreateAsync(new ShellSettings { Theme = ThemeMode.Dark, AccentOverride = "#4CC2FF" });

        await service.ApplyCurrentAsync(TestContext.Current.CancellationToken);

        Assert.Single(applier.Applied);
        var accent = Assert.Single(applier.Accents);
        Assert.Equal("#4CC2FF", accent.Hex);
        Assert.Equal(ApplicationTheme.Dark, accent.Theme);
    }

    [Fact]
    public async Task NoAccentOverride_RestoresSystemAccent()
    {
        var (service, applier) = await CreateAsync(new ShellSettings { Theme = ThemeMode.Light });

        await service.ApplyCurrentAsync(TestContext.Current.CancellationToken);

        var accent = Assert.Single(applier.Accents);
        Assert.Null(accent.Hex);
        Assert.Equal(ApplicationTheme.Light, accent.Theme);
    }
}
