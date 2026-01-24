namespace FantaniaLib;

public struct TextureParameterUnion
{
    public ImageParameter ImageParams { get; set; }
    public AtlasParameter AtlasParams { get; set; }
    public GpuParameter GpuParams { get; set; }
}

public struct ImageParameter : IEquatable<ImageParameter>
{
    public string ImagePath { get; set; }

    public bool Equals(ImageParameter other)
    {
        return ImagePath == other.ImagePath;
    }

    public override string ToString()
    {
        return ImagePath;
    }
}

public struct AtlasParameter : IEquatable<AtlasParameter>
{
    public string AtlasPath { get; set; }
    public string FrameKey { get; set; }

    public bool Equals(AtlasParameter other)
    {
        return AtlasPath == other.AtlasPath && FrameKey == other.FrameKey;
    }

    public override string ToString()
    {
        return $"{AtlasPath}:{FrameKey}";
    }
}

public struct GpuParameter : IEquatable<GpuParameter>
{
    public int TextureID { get; set; }

    public bool Equals(GpuParameter other)
    {
        return TextureID == other.TextureID;
    }

    public override string ToString()
    {
        return $"{TextureID}";
    }
}