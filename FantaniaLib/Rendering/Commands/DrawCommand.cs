namespace FantaniaLib;

public class DrawCommand : IRenderCommand
{
    public IEnumerable<Mesh> Meshes { get; private set; }
    public RenderMaterial Material { get; private set; }

    public DrawCommand(IEnumerable<Mesh> meshes, RenderMaterial material)
    {
        Meshes = meshes;
        Material = material;
    }

    public void Execute(ConfigurableRenderPipeline pipeline)
    {
        pipeline.SyncGlobalUniforms(Material);
        VertexDescriptor desc = Meshes.First().Descriptor.VertexDescriptor;
        VertexStream stream = pipeline.VertexStreamCache.Acquire(desc);
        foreach (var mesh in Meshes)
        {
            if (!stream.TryAppend(mesh))
            {
                pipeline.Device.SyncVertexStream(stream);
                pipeline.Device.Draw(stream, Material);
                stream.Reset();
                stream.TryAppend(mesh);
            }
        }
        if (stream.IndiceCount > 0)
        {
            pipeline.Device.SyncVertexStream(stream);
            pipeline.Device.Draw(stream, Material);
        }
        pipeline.VertexStreamCache.Recycle(stream);
    }
}