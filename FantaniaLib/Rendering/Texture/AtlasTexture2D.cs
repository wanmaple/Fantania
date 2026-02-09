namespace FantaniaLib;

public class AtlasTexture2D : ITexture2D
{
    public TextureCategory Category => TextureCategory.Atlas;
    public string Identifier { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public ColorSpaceTypes ColorSpace { get; private set; } = ColorSpaceTypes.Unknown;
    public TextureFormats Format { get; private set; } = TextureFormats.Unknown;
    public Recti TextureRect { get; private set; } = Recti.Zero;

    public bool IsValid { get; private set; } = false;

    public AtlasTexture2D(SpriteAtlas atlas, string key)
    {
        Identifier = atlas.Identifier;
        _atlas = atlas;
        _frameKey = key;
        if (atlas.IsValid)
        {
            Analyze();
        }
    }

    void Analyze()
    {
        string atlasPath = _atlas.AtlasPath;
        Stream? stream = null;
        if (atlasPath.StartsWith("avares://"))
            stream = AvaloniaHelper.ReadAssetStream(atlasPath);
        else if (File.Exists(atlasPath))
            stream = File.OpenRead(atlasPath);
        if (stream != null && ImageAnalysis.AnalysisStream(stream, out var result))
        {
            Width = result.Width;
            Height = result.Height;
            ColorSpace = result.ColorSpace;
            if (result.IsSrgb)
                Format = result.HasAlpha ? TextureFormats.SRGB8_ALPHA8 : TextureFormats.SRGB8;
            else
                Format = result.HasAlpha ? TextureFormats.RGBA8 : TextureFormats.RGB8;
            TextureRect = _atlas.GetFrame(_frameKey);
            if (TextureRect != Recti.Zero)
                IsValid = true;
        }
    }

    public bool TryDecode(out TextureDescription desc, out byte[]? data)
    {
        desc = new TextureDescription();
        data = null;
        string path = _atlas.AtlasPath;
        Stream? stream = null;
        if (path.StartsWith("avares://"))
            stream = AvaloniaHelper.ReadAssetStream(path);
        else if (File.Exists(path))
            stream = File.OpenRead(path);
        if (stream != null)
        {
            using (stream)
            {
                data = ImageAnalysis.DecodeBytes(stream);
                if (data != null)
                {
                    desc.Width = Width;
                    desc.Height = Height;
                    desc.Format = Format;
                    desc.GenerateMipmap = true;
                    desc.WrapS = TextureWraps.ClampToEdge;
                    desc.WrapT = TextureWraps.ClampToEdge;
                    return true;
                }
            }
        }
        return false;
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, $"Size: ({TextureRect.Width}, {TextureRect.Height})", $"Format: {Format}", $"Color Space: {ColorSpace}");
    }

    SpriteAtlas _atlas;
    string _frameKey;
}