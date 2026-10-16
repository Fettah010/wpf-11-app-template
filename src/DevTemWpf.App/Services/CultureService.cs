using System.Globalization;

namespace DevTemWpf.App.Services;

/// <summary>
/// Culture selection (restart-applied; WPF static resources don't refresh).
/// Sets all four culture slots: stale threads keep their birth culture
/// otherwise, which once left page construction on en-US while VM lookups
/// already saw German (see 1.3.0 report).
/// </summary>
public static class CultureService
{
    public static bool ApplyCulture(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return true;
        }

        try
        {
            var culture = new CultureInfo(cultureName.Trim());
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    /// <summary>Parses a culture name for <see cref="Resources.Strings.OverrideCulture"/> (null when unset/unknown).</summary>
    public static CultureInfo? CultureOrNull(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return null;
        }

        try
        {
            return new CultureInfo(cultureName.Trim());
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
