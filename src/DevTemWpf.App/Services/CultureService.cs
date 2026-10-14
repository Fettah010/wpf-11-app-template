using System.Globalization;

namespace DevTemWpf.App.Services;

/// <summary>Culture selection (restart-applied; WPF static resources don't refresh).</summary>
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
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
