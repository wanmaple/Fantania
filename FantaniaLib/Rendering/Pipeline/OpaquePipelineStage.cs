namespace FantaniaLib;

public class OpaquePipelineStage : IPipelineStage
{
    public string Name => "Opaque";
    public int Order => 2000;

    public void PostRender(IRenderContext context)
    {
    }

    public void PreRender(IRenderContext context)
    {
        context.CommandBuffer.SetupState(_state);
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables)
    {
        var list = renderables.ToList();
        list.StableSort(RenderableDepthComparer.Instance);
        var groupDict = new Dictionary<(VertexDescriptor, RenderMaterial), (List<Mesh>, RenderMaterial)>();
        foreach (var renderable in list)
        {
            var key = (renderable.Mesh.Descriptor.VertexDescriptor, renderable.Material);
            if (!groupDict.TryGetValue(key, out var drawInfo))
            {
                drawInfo = (new List<Mesh>(), renderable.Material);
                groupDict.Add(key, drawInfo);
            }
            drawInfo.Item1.Add(renderable.Mesh);
        }
        foreach (var pair in groupDict)
        {
            context.CommandBuffer.Draw(pair.Value.Item1, pair.Value.Item2);
        }
    }

    readonly RenderState _state = new RenderState
    {
        DepthTestEnabled = true,
        DepthWriteEnabled = true,
        BlendingEnabled = false,
    };
}