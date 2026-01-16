namespace FantaniaLib;

public static class IOHelper
{
    public static Stream? ReadStream(string path, IWorkspace workspace)
    {
        if (path.StartsWith("avares://"))
        {
            return AvaloniaHelper.ReadAssetStream(path);
        }
        else if (File.Exists(path))
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }
        else
        {
            string fullPath = workspace.GetAbsolutePath(path);
            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            }
        }
        return null;
    }

    public static string? ReadText(string path, IWorkspace workspace)
    {
        var stream = ReadStream(path, workspace);
        if (stream == null) return null;
        using (stream)
        using (var sr = new StreamReader(stream))
            return sr.ReadToEnd();
    }

    public static async Task<string?> ReadTextAsync(string path, IWorkspace workspace)
    {
        var stream = ReadStream(path, workspace);
        if (stream == null) return null;
        using (stream)
        using (var sr = new StreamReader(stream))
            return await sr.ReadToEndAsync();
    }

    public static byte[]? ReadBytes(string path, IWorkspace workspace)
    {
        var stream = ReadStream(path, workspace);
        if (stream == null) return null;
        using (stream)
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}