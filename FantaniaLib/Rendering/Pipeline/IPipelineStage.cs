namespace FantaniaLib;

[BindingScript]
public interface IPipelineStage
{
    string Name { get; }
    int Order { get; }

    void PreRender(IRenderDevice device);
    void Render(IRenderDevice device);
    void PostRender(IRenderDevice device);
}