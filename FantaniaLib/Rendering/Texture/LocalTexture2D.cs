namespace FantaniaLib;

public class LocalTexture2D : ITexture2D
{
    public TextureCategory Category => TextureCategory.Local;
    public string Identifier { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public ColorSpaceTypes ColorSpace { get; private set; } = ColorSpaceTypes.Unknown;
    public TextureFormats Format { get; private set; } = TextureFormats.Unknown;
    public Recti TextureRect { get; private set; }

    public bool IsValid { get; private set; } = false;

    public LocalTexture2D(string path)
    {
        Identifier = path.ToStandardPath();
        Stream? stream = null;
        if (path.StartsWith("avares://"))
            stream = AvaloniaHelper.ReadAssetStream(path);
        else if (File.Exists(path))
            stream = File.OpenRead(path);
        if (stream != null)
        {
            using (stream)
                LoadFromStream(stream);
        }
    }

    void LoadFromStream(Stream stream)
    {
        IsValid = ImageAnalysis.AnalysisStream(stream, out var result);
        if (IsValid)
        {
            Width = result.Width;
            Height = result.Height;
            ColorSpace = result.ColorSpace;
            if (result.IsSrgb)
                Format = result.HasAlpha ? TextureFormats.SRGB8_ALPHA8 : TextureFormats.SRGB8;
            else
                Format = result.HasAlpha ? TextureFormats.RGBA8 : TextureFormats.RGB8;
            TextureRect = new Recti(0, 0, Width, Height);
        }
    }

    public bool TryDecode(out TextureDescription desc, out byte[]? data)
    {
        desc = new TextureDescription();
        data = null;
        string path = Identifier;
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
                    desc.WrapS = TextureWraps.Repeat;
                    desc.WrapT = TextureWraps.Repeat;
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
}