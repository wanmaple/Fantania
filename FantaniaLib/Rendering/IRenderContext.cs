namespace FantaniaLib;

public interface IRenderContext
{
    CommandBuffer CommandBuffer { get; }
    ShaderCache ShaderCache { get; }
    TextureManager TextureManager { get; }
    MaterialSet MaterialSet { get; }
    VertexStreamCache VertexStreamCache { get; }
    UniformSet GlobalUniforms { get; }
    int LightCullingTileSize { get; }
    TiledLightCullingData TiledLightCullingData { get; }

    int MaxTextureSlot { get; }

    FrameBuffer? GetFrameBuffer(string name);
}