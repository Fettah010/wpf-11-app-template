using DevTemWpf.App.Services;
using DevTemWpf.App.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DevTemWpf.Unit;

public sealed class SettingsViewModelTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    public void Dispose()
    {
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    private sealed class FakeApplier : IThemeApplier
    {
        public void Apply(ApplicationTheme theme, WindowBackdropType backdrop) { }
        public ApplicationTheme GetSystemApplicationTheme() => ApplicationTheme.Dark;
        public void ApplyAccentOverride(string? hex, ApplicationTheme theme) { }
        public bool IsHighContrast() => false;
    }

    [Theory]
    [InlineData("#4CC2FF", true)]
    [InlineData("#FF4CC2FF", true)]
    [InlineData("not-a-color", false)]
    [InlineData("#12345", false)]
    [InlineData("", false)]
    public void IsValidHex_AcceptsRgbAndArgbOnly(string hex, bool expected)
    {
        Assert.Equal(expected, SettingsViewModel.IsValidHex(hex));
    }

    [Fact]
    public async Task Reload_LoadsPersistedDensityAndAccent()
    {
        var settings = new SettingsService(_path);
        await settings.UpdateAsync(
            _ => new ShellSettings { Density = DensityMode.Compact, AccentOverride = "#4CC2FF" },
            TestContext.Current.CancellationToken);
        var theme = new ThemeService(settings, new BackdropService(() => true, () => false), new FakeApplier());

        var vm = new SettingsViewModel(settings, theme);

        Assert.Equal(DensityMode.Compact, vm.SelectedDensity);
        Assert.True(vm.UseCustomAccent);
        Assert.Equal("#4CC2FF", vm.AccentHex);
    }
}
