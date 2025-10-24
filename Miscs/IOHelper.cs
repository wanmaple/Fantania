using System;
using System.IO;
using Avalonia.Platform;

namespace Fantania;

public static class IOHelper
{
    public static string ReadAllText(string path)
    {
        if (path.StartsWith("avares://"))
        {
            var uri = new Uri(path);
            using (var assets = AssetLoader.Open(uri))
            {
                using (var sr = new StreamReader(assets))
                {
                    string source = sr.ReadToEnd();
                    return source;
                }
            }
        }
        else if (File.Exists(path))
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    string source = sr.ReadToEnd();
                    return source;
                }
            }
        }
        return null;
    }

    public static byte[] ReadAllBytes(string path)
    {
        if (path.StartsWith("avares://"))
        {
            var uri = new Uri(path);
            using (var assets = AssetLoader.Open(uri))
            {
                using (var ms = new MemoryStream())
                {
                    assets.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
        else if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        return null;
    }
}