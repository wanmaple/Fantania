namespace FantaniaLib;

[BindingScript]
public enum TextureFormats
{
    Unknown,
    R8,
    RGB8,
    RGBA8,
    SRGB8,
    SRGB8_ALPHA8,
    R16F,
    RG16F,
    RGBA16F,
}

[BindingScript]
public enum TextureMinFilters
{
    Nearest,
    Linear,
    NearestMipmapNearest,
    LinearMipmapNearest,
    NearestMipmapLinear,
    LinearMipmapLinear,
}

[BindingScript]
public enum TextureMagFilters
{
    Nearest,
    Linear,
}

[BindingScript]
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