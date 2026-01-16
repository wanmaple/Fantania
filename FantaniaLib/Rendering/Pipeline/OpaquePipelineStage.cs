
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
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables)
    {
    }

    readonly RenderState _state = new RenderState
    {
        DepthTestEnabled = true,
        DepthWriteEnabled = true,
        BlendingEnabled = false,
    };
}