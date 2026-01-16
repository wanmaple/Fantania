
namespace FantaniaLib;

public class TransparentPipelineStage : IPipelineStage
{
    public string Name => "Transparent";
    public int Order => 4000;

    public void PostRender(IRenderContext context)
    {
    }

    public void PreRender(IRenderContext context)
    {
        context.CommandBuffer.SetupState(_state);
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables)
    {
        var groups = renderables.GroupBy(r => (r.Mesh.Descriptor.VertexDescriptor, r.Material));
        foreach (var group in groups)
        {
            var vertDesc = group.Key.VertexDescriptor;
            var material = group.Key.Material;
            context.CommandBuffer.Draw(group.Select(r => r.Mesh), material);
        }
    }

    readonly RenderState _state = new RenderState
    {
        DepthTestEnabled = true,
        DepthWriteEnabled = false,
        BlendingEnabled = true,
        BlendFuncSrc = BlendFuncs.SrcAlpha,
        BlendFuncDst = BlendFuncs.OneMinusSrcAlpha,
    };
}