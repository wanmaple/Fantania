namespace FantaniaLib;

[BindingScript]
public interface IPipelineStage
{
    string Name { get; }
    int Order { get; }

    void Setup(IRenderContext context);
    void PreRender(IRenderContext context);
    void Render(IRenderContext context, IEnumerable<IRenderable> renderables, Camera2DFrameData camData);
    void PostRender(IRenderContext context);
}