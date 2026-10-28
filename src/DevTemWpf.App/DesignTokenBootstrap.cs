using System.Windows;
using DevTemWpf.App.Services;

namespace DevTemWpf.App;

/// <summary>
/// Density dictionary swap (ADR-0007). Spacing values are always consumed via
/// StaticResource from the active density dictionary, so replacing the whole
/// dictionary updates every consumer. Falls back to Comfortable.
/// </summary>
internal static class DesignTokenBootstrap
{
    private static DensityMode _applied = DensityMode.Comfortable;

    internal static void ApplyDensity(DensityMode density)
    {
        var app = Application.Current;
        if (app is null || density == _applied && HasDensityDictionary(app, density))
        {
            _applied = density;
            return;
        }

        var merged = app.Resources.MergedDictionaries;
        for (var i = merged.Count - 1; i >= 0; i--)
        {
            var source = merged[i].Source?.OriginalString ?? string.Empty;
            if (source.Contains("Spacing.", StringComparison.OrdinalIgnoreCase))
            {
                merged.RemoveAt(i);
            }
        }

        try
        {
            merged.Add(new ResourceDictionary
            {
                Source = new Uri($"/DevTemWpf.UI;component/DesignTokens/Spacing.{density}.xaml", UriKind.Relative),
            });
            _applied = density;
        }
        catch
        {
            // Defensive: never break startup over the density layer.
            merged.Add(new ResourceDictionary
            {
                Source = new Uri("/DevTemWpf.UI;component/DesignTokens/Spacing.Comfortable.xaml", UriKind.Relative),
            });
            _applied = DensityMode.Comfortable;
        }
    }

    private static bool HasDensityDictionary(Application app, DensityMode density)
    {
        foreach (var dictionary in app.Resources.MergedDictionaries)
        {
            if ((dictionary.Source?.OriginalString ?? string.Empty).Contains(
                $"Spacing.{density}.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
