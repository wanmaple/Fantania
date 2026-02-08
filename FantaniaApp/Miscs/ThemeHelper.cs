using Avalonia;
using Avalonia.Media;

namespace Fantania;

public static class ThemeHelper
{
    public static IBrush BackgroundBrush
    {
        get
        {
            var currentTheme = Application.Current!.ActualThemeVariant;
            Application.Current.TryGetResource("SystemAltHighColor", currentTheme, out var color);
            return new SolidColorBrush((Color)color!);
        }
    }

    public static IBrush ForegroundBrush
    {
        get
        {
            var currentTheme = Application.Current!.ActualThemeVariant;
            Application.Current.TryGetResource("SystemBaseHighColor", currentTheme, out var color);
            return new SolidColorBrush((Color)color!);
        }
    }
}