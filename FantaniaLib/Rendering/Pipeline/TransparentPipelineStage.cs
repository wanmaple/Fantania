namespace FantaniaLib;

public class TransparentPipelineStage : IPipelineStage
{
    public string Name => "Transparent";

    public int Order => 4000;

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