using System.Text.Json;

namespace FantaniaLib;

public class SpriteAtlas
{
    public string Identifier { get; private set; }
    public string AtlasPath { get; private set; } = string.Empty;
    public int AtlasWidth { get; private set; }
    public int AtlasHeight { get; private set; }
    public bool IsValid => _keys != null && _keys.Count > 0;

    public IReadOnlyCollection<string> Keys => IsValid ? _frameMap!.Keys : Array.Empty<string>();

    public SpriteAtlas(string path)
    {
        Identifier = path;
        string json = string.Empty;
        if (path.StartsWith("avares://"))
            json = AvaloniaHelper.ReadAssetText(path);
        else if (File.Exists(path))
            json = File.ReadAllText(path);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                BuildAtlas(json, Path.GetDirectoryName(path)!);
            }
            catch (Exception)
            { }
        }
    }

    public Recti GetFrame(string key)
    {
        if (string.IsNullOrEmpty(key)) return Recti.Zero;
        if (!IsValid) return Recti.Zero;
        if (_frameMap!.TryGetValue(key, out var frame))
            return frame;
        return Recti.Zero;
    }

    void BuildAtlas(string json, string folder)
    {
        JsonDocument doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var frames = root.GetProperty("frames");
        var meta = root.GetProperty("meta");
        string atlasRelativePath = meta.GetProperty("image").GetString()!;
        var size = meta.GetProperty("size");
        AtlasPath = Path.Join(folder, atlasRelativePath).ToStandardPath();
        AtlasWidth = size.GetProperty("w").GetInt32();
        AtlasHeight = size.GetProperty("h").GetInt32();
        _frameMap = new Dictionary<string, Recti>(frames.GetArrayLength());
        _keys = new List<string>(frames.GetArrayLength());
        for (int i = 0; i < frames.GetArrayLength(); i++)
        {
            var frameData = frames[i];
            string key = frameData.GetProperty("filename").GetString()!;
            key = Path.GetFileNameWithoutExtension(key);
            var rect = frameData.GetProperty("frame");
            int x = rect.GetProperty("x").GetInt32();
            int y = rect.GetProperty("y").GetInt32();
            int w = rect.GetProperty("w").GetInt32();
            int h = rect.GetProperty("h").GetInt32();
            Recti frame = new Recti(x, y, w, h);
            _frameMap.Add(key, frame);
            _keys.Add(key);
        }
    }

    Dictionary<string, Recti>? _frameMap;
    List<string>? _keys;
}