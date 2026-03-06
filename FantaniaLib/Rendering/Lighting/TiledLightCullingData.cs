using System.Numerics;

namespace FantaniaLib;

public class TiledLightCullingData
{
    public int TileSize { get; set; }
    public int RenderWidth { get; set; }
    public int RenderHeight { get; set; }
    public int TilesX { get; set; }
    public int TilesY { get; set; }
    public int[] TileOffsets { get; set; } = Array.Empty<int>();
    public int[] TileCounts { get; set; } = Array.Empty<int>();
    public int[] TileLightIndices { get; set; } = Array.Empty<int>();
    public Vector4[] LightPosRadius { get; set; } = Array.Empty<Vector4>();
    public Vector4[] LightColors { get; set; } = Array.Empty<Vector4>();
    public int[] LightLayers { get; set; } = Array.Empty<int>();
    public TextureDefinition[] LightTextures { get; set; } = Array.Empty<TextureDefinition>();
    public int[] LightTextureSlots { get; set; } = Array.Empty<int>();
    public int[] LightTextureIndices { get; set; } = Array.Empty<int>();
}
