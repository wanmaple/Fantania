namespace FantaniaLib;

public class FrameBufferConfig
{
    public required string Name { get; set; }
    public required FrameBufferDescription Description { get; set; }
}

public struct MaterialInfo
{
    public required string MaterialKey { get; set; }
    public required string VertexShader { get; set; }
    public required string FragmentShader { get; set; }
}

public struct TextureFilters : IEquatable<TextureFilters>
{
    public static TextureFilters PixelClamp => new TextureFilters
    {
        MinFilter = TextureMinFilters.Nearest,
        MagFilter = TextureMagFilters.Nearest,
        WrapS = TextureWraps.ClampToEdge,
        WrapT = TextureWraps.ClampToEdge,
    };
    public static TextureFilters PixelRepeat => new TextureFilters
    {
        MinFilter = TextureMinFilters.Nearest,
        MagFilter = TextureMagFilters.Nearest,
        WrapS = TextureWraps.Repeat,
        WrapT = TextureWraps.Repeat,
    };
    public static TextureFilters LinearClamp => new TextureFilters
    {
        MinFilter = TextureMinFilters.Linear,
        MagFilter = TextureMagFilters.Linear,
        WrapS = TextureWraps.ClampToEdge,
        WrapT = TextureWraps.ClampToEdge,
    };
    public static TextureFilters LinearRepeat => new TextureFilters
    {
        MinFilter = TextureMinFilters.Linear,
        MagFilter = TextureMagFilters.Linear,
        WrapS = TextureWraps.Repeat,
        WrapT = TextureWraps.Repeat,
    };

    public required TextureMinFilters MinFilter { get; set; }
    public required TextureMagFilters MagFilter { get; set; }
    public required TextureWraps WrapS { get; set; }
    public required TextureWraps WrapT { get; set; }

    public static bool operator ==(TextureFilters left, TextureFilters right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextureFilters left, TextureFilters right)
    {
        return !(left == right);
    }

    public bool Equals(TextureFilters other)
    {
        return MinFilter == other.MinFilter && MagFilter == other.MagFilter && WrapS == other.WrapS && WrapT == other.WrapT;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextureFilters other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MinFilter, MagFilter, WrapS, WrapT);
    }

    public override string ToString()
    {
        return $"MinFilter: {MinFilter}, MagFilter: {MagFilter}, WrapS: {WrapS}, WrapT: {WrapT}";
    }
}

public class RenderPipelineConfig
{
    public required Vector2Int Resolution { get; set; }
    public required TextureFilters DefaultTextureFilter { get; set; }
    public required int LightCullingTileSize { get; set; }
    public required IReadOnlyList<FrameBufferConfig> FrameBuffers { get; set; }
    public required IReadOnlyList<IPipelineStage> Stages { get; set; }
    public required IReadOnlyList<MaterialInfo> Materials { get; set; }
}