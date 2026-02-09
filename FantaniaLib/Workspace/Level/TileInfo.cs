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
    };

    public string RenderStage;
    public string MaterialKey;
    public DesiredUniformMap Uniforms;
    public Vector2 UVOffset;
    public Vector2 UVSize;
}