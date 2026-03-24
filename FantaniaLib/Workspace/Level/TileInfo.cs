using System.Numerics;

namespace FantaniaLib;

public struct TileInfo
{
    public static readonly TileInfo Default = new TileInfo
    {
        RenderStage = string.Empty,
        MaterialKey = string.Empty,
        Uniforms = new DesiredUniformMap(),
        UVOffset = Vector2.Zero,
        UVSize = Vector2.One,
        Color = Vector4.One,
        OverrideTextureFilters = new Dictionary<string, TextureFilters>(),
    };

    public string RenderStage;
    public string MaterialKey;
    public DesiredUniformMap Uniforms;
    public Vector2 UVOffset;
    public Vector2 UVSize;
    public Vector4 Color;
    public IReadOnlyDictionary<string, TextureFilters> OverrideTextureFilters;
}