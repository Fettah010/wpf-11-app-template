using System.Globalization;
using DevTemWpf.App.Resources;
using DevTemWpf.App.Services;

namespace DevTemWpf.Unit;

public sealed class LocalizationTests
{
    [Fact]
    public void English_IsDefault()
    {
        Assert.Equal("Home", Strings.Get("Nav_Home", CultureInfo.GetCultureInfo("en")));
        Assert.Equal("Settings", Strings.Get("Nav_Settings", CultureInfo.GetCultureInfo("en")));
    }

    [Fact]
    public void German_ResolvesFromSatellite()
    {
        Assert.Equal("Start", Strings.Get("Nav_Home", CultureInfo.GetCultureInfo("de")));
        Assert.Equal("Einstellungen", Strings.Get("Nav_Settings", CultureInfo.GetCultureInfo("de")));
    }

    [Fact]
    public void MissingKey_ReturnsMarker()
    {
        Assert.Equal("!Nope!", Strings.Get("Nope", CultureInfo.GetCultureInfo("en")));
    }

    [Fact]
    public void ApplyCulture_AcceptsEmpty_RejectsUnknown()
    {
        Assert.True(CultureService.ApplyCulture(string.Empty));
        Assert.False(CultureService.ApplyCulture("xx-invalid-culture"));
    }
}
