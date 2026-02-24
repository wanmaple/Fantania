namespace FantaniaLib;

public class DrawCommand : IRenderCommand
{
    public List<Mesh> Meshes { get; private set; }
    public RenderMaterial Material { get; private set; }

    public DrawCommand(List<Mesh> meshes, RenderMaterial material)
    {
        Meshes = meshes;
        Material = material;
    }

    public void Execute(ConfigurableRenderPipeline pipeline)
    {
        VertexDescriptor desc = Meshes.First().Descriptor.VertexDescriptor;
        VertexStream stream = pipeline.VertexStreamCache.Acquire(desc);
        foreach (var mesh in Meshes)
        {
            if (!stream.TryAppend(mesh))
            {
                pipeline.Device.SyncVertexStream(stream);
                pipeline.Device.Draw(stream, Material.Shader, Material.Uniforms, pipeline.GlobalUniforms);
                ++pipeline.Statistics.DrawCalls;
                pipeline.Statistics.Triangles += stream.IndiceCount / 3;
                stream.Reset();
                stream.TryAppend(mesh);
            }
        }
        if (stream.IndiceCount > 0)
        {
            pipeline.Device.SyncVertexStream(stream);
            pipeline.Device.Draw(stream, Material.Shader, Material.Uniforms, pipeline.GlobalUniforms);
            ++pipeline.Statistics.DrawCalls;
            pipeline.Statistics.Triangles += stream.IndiceCount / 3;
        }
        pipeline.VertexStreamCache.Recycle(stream);
    }
}