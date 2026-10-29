using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UiPackageConsumer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var margin = (Thickness)FindResource("AppPageMargin");
            if (margin != new Thickness(24))
            {
                throw new InvalidOperationException($"Unexpected default page margin: {margin}.");
            }

            var dictionaries = Resources.MergedDictionaries;
            for (var i = dictionaries.Count - 1; i >= 0; i--)
            {
                if ((dictionaries[i].Source?.OriginalString ?? string.Empty)
                    .Contains("Spacing.", StringComparison.OrdinalIgnoreCase))
                {
                    dictionaries.RemoveAt(i);
                }
            }

            dictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(
                    "/DevTemWpf.UI;component/DesignTokens/Spacing.Compact.xaml",
                    UriKind.Relative),
            });
            margin = (Thickness)FindResource("AppPageMargin");
            if (margin != new Thickness(16))
            {
                throw new InvalidOperationException($"Unexpected compact page margin: {margin}.");
            }

            var window = new Window
            {
                Width = 480,
                Height = 240,
                Content = new TextBlock
                {
                    Text = "DevTemWpf.UI package resource loaded",
                    Style = (Style)FindResource("AppTitleText"),
                    Margin = margin,
                },
            };
            window.Loaded += (_, _) => Shutdown(0);
            window.Show();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            Shutdown(1);
        }
    }
}
