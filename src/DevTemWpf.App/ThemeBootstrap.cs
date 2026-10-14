using System.Windows;

namespace DevTemWpf.App;

/// <summary>
/// Version-sensitive WPF-UI theme seam (ADR-0001).
/// Verifies the 0.1.0 prototype dictionaries resolved; the 0.3.0 token
/// layer replaces this class without touching ViewModels.
/// </summary>
internal static class ThemeBootstrap
{
    internal static bool EnsureLoaded(Application app)
    {
        foreach (var dictionary in app.Resources.MergedDictionaries)
        {
            // ThemesDictionary/ControlsDictionary merge their keys here.
            if (dictionary.Count > 0)
            {
                return true;
            }
        }

        return false;
    }
}
