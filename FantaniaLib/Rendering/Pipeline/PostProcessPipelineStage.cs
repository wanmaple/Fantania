
namespace FantaniaLib;

public class PostProcessPipelineStage : IPipelineStage
{
    public string Name => "Post Processing";
    public int Order => 10000;

    public void Setup(IRenderContext context)
    {
    }

    public void PostRender(IRenderContext context)
    {
    }

    public void PreRender(IRenderContext context)
    {
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables, Camera2DFrameData camData)
    {
    }

    readonly RenderState _state = new RenderState
    {
        DepthTestEnabled = false,
        DepthWriteEnabled = false,
        BlendingEnabled = false,
    };
}