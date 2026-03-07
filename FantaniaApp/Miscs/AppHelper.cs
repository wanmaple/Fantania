using Avalonia;

namespace Fantania;

public static class AppHelper
{
    public static AppSettings Settings => ((App)Application.Current!).Settings;
}
