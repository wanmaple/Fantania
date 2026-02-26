namespace FantaniaLib;

[BindingScript]
public interface IPipelineStage
{
    string Name { get; }
    int Order { get; }

    void PreRender(IRenderContext context);
    void Render(IRenderContext context, IEnumerable<IRenderable> renderables, Camera2D camera);
    void PostRender(IRenderContext context);
}