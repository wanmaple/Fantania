namespace FantaniaLib;

public enum TextureCategory
{
    Local,
    Atlas,
    Custom,
}

public interface ITexture2D
{
    TextureCategory Category { get; }
    string Identifier { get; }
    int Width { get; }
    int Height { get; }
    ColorSpaceTypes ColorSpace { get; }
    TextureFormats Format { get; }
    Recti TextureRect { get; }
    bool IsValid { get; }

    bool TryDecode(out TextureDescription desc, out byte[]? data);
}