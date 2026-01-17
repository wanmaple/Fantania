using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace FantaniaLib;

public static class AvaloniaHelper
{
    public static string ConvertAvaloniaUriToStandardUri(string uri)
    {
        if (OperatingSystem.IsWindows())
            uri = uri.Replace("file:///", string.Empty);
        else if (OperatingSystem.IsMacOS())
            uri = uri.Replace("file://", string.Empty);
        return uri;
    }
    
    public static IEnumerable<string> EnumerateAssetFolder(string folder)
    {
        if (folder.StartsWith("avares://"))
        {
            var uri = new Uri(folder);
            var assets = AssetLoader.GetAssets(uri, null);
            foreach (var asset in assets)
            {
                yield return asset.ToString();
            }
        }
    }

    public static Stream? ReadAssetStream(string assetPath)
    {
        var uri = new Uri(assetPath);
        return AssetLoader.Open(uri);
    }

    public static string ReadAssetText(string assetPath)
    {
        var uri = new Uri(assetPath);
        using (var assets = AssetLoader.Open(uri))
        {
            using (var sr = new StreamReader(assets))
            {
                string source = sr.ReadToEnd();
                return source;
            }
        }
    }

    public static byte[] ReadAssetBytes(string assetPath)
    {
        var uri = new Uri(assetPath);
        using (var assets = AssetLoader.Open(uri))
        {
            using (var ms = new MemoryStream())
            {
                assets.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    public static Window GetTopWindow()
    {
        var desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        return desktop.MainWindow!;
    }
}