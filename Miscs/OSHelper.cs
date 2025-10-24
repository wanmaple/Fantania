using System;

namespace Fantania;

public static class OSHelper
{
    public static string ConvertAvaloniaUriToStandardUri(string uri)
    {
        if (OperatingSystem.IsWindows())
            uri = uri.Replace("file:///", string.Empty);
        else if (OperatingSystem.IsMacOS())
            uri = uri.Replace("file://", string.Empty);
        return uri;
    }
}