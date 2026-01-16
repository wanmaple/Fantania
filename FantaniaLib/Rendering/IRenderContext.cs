namespace FantaniaLib;

public interface IRenderContext
{
    CommandBuffer CommandBuffer { get; }
    ShaderCache ShaderCache { get; }
    TextureManager TextureManager { get; }
    MaterialSet MaterialSet { get; }
    VertexStreamCache VertexStreamCache { get; }

    void SyncGlobalUniforms(RenderMaterial material);
}