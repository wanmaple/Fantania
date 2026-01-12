namespace FantaniaLib;

public enum TextureFormats
{
    R8,
    RGB8,
    RGBA8,
    SRGB8,
}

public enum TextureMinFilters
{
    Nearest,
    Linear,
    NearestMipmapNearest,
    LinearMipmapNearest,
    NearestMipmapLinear,
    LinearMipmapLinear,
}

public enum TextureMagFilters
{
    Nearest,
    Linear,
}

public enum TextureWraps
{
    ClampToEdge,
    ClampToBorder,
    Repeat,
    MirroredRepeat,
    MirrorClampToEdge,
}

public class TextureDescription
{
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
    public TextureFormats Format { get; set; } = TextureFormats.RGBA8;
    public bool GenerateMipmap { get; set; } = false;
    public TextureMinFilters MinFilter { get; set; } = TextureMinFilters.Linear;
    public TextureMagFilters MagFilter { get; set; } = TextureMagFilters.Linear;
    public TextureWraps WrapS { get; set; } = TextureWraps.ClampToEdge;
    public TextureWraps WrapT { get; set; } = TextureWraps.ClampToEdge;
}