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

public class RenderPipelineConfig
{
    public required Vector2Int Resolution { get; set; }
    public required int LightCullingTileSize { get; set; }
    public required IReadOnlyList<FrameBufferConfig> FrameBuffers { get; set; }
    public required IReadOnlyList<IPipelineStage> Stages { get; set; }
    public required IReadOnlyList<MaterialInfo> Materials { get; set; }
}