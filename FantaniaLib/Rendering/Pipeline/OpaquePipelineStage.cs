namespace FantaniaLib;

public class OpaquePipelineStage : IPipelineStage
{
    public string Name => "Opaque";
    public int Order => 2000;

    public void PostRender(IRenderDevice device)
    {
    }

    public void PreRender(IRenderDevice device)
    {
    }

    public void Render(IRenderDevice device)
    {
    }
}