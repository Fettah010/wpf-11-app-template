using System.Windows;
using System.Windows.Controls;
using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class MotionServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    private async Task<MotionService> CreateAsync(ShellSettings seed)
    {
        var settings = new SettingsService(_path);
        await settings.UpdateAsync(_ => seed, TestContext.Current.CancellationToken);
        return new MotionService(settings, osAnimationsEnabled: () => true, renderTier: () => 2);
    }

    [WpfFact]
    public async Task Entrance_OffPolicy_SnapsFinalState()
    {
        var service = await CreateAsync(new ShellSettings { Motion = MotionMode.Off });
        var element = new Border { Opacity = 0 };

        await service.PlayEntranceAsync(element, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, element.Opacity);
    }

    [WpfFact]
    public async Task Entrance_DurationOverride_StillInstantWhenOff()
    {
        var service = await CreateAsync(new ShellSettings { Motion = MotionMode.Off });
        var element = new Border { Opacity = 0 };

        await service.PlayEntranceAsync(element, MotionTokens.Deliberate, TestContext.Current.CancellationToken);

        Assert.Equal(1, element.Opacity);
    }

    [WpfFact]
    public async Task Ripple_AddsAndRemovesEllipse()
    {
        var service = await CreateAsync(new ShellSettings { Motion = MotionMode.Full, EffectsEnabled = true });
        var canvas = new Canvas { Width = 300, Height = 200 };

        service.PlayRipple(canvas, new Point(150, 100));

        Assert.Single(canvas.Children);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (canvas.Children.Count > 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        Assert.Empty(canvas.Children);
    }

    [WpfFact]
    public async Task Ripple_SkippedWhenEffectsDisabled()
    {
        var service = await CreateAsync(new ShellSettings { Motion = MotionMode.Full, EffectsEnabled = false });
        var canvas = new Canvas { Width = 300, Height = 200 };

        service.PlayRipple(canvas, new Point(10, 10));

        Assert.Empty(canvas.Children);
    }

    [WpfFact]
    public async Task AllowsEffect_Ripple_FalseAtBalanced_TrueAtRich()
    {
        var balanced = await CreateAsync(new ShellSettings { Motion = MotionMode.Balanced });
        Assert.False(balanced.AllowsEffect(MotionEffect.Ripple));

        var rich = await CreateAsync(new ShellSettings { Motion = MotionMode.Full });
        Assert.True(rich.AllowsEffect(MotionEffect.Ripple));
    }

    [WpfFact]
    public async Task AllowsEffect_Ripple_RespectsPerEffectSwitch()
    {
        var service = await CreateAsync(new ShellSettings
        {
            Motion = MotionMode.Full,
            RippleEffectEnabled = false,
        });

        Assert.False(service.AllowsEffect(MotionEffect.Ripple));
    }

    [WpfFact]
    public async Task AllowsEffect_Warp_OffUntilExplicitlyEnabled()
    {
        var service = await CreateAsync(new ShellSettings { Motion = MotionMode.Full });
        Assert.False(service.AllowsEffect(MotionEffect.Warp));

        var enabled = await CreateAsync(new ShellSettings
        {
            Motion = MotionMode.Full,
            WarpEffectEnabled = true,
        });
        Assert.True(enabled.AllowsEffect(MotionEffect.Warp));
    }
}
