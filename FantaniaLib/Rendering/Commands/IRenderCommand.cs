namespace FantaniaLib;

public interface IRenderCommand
{
    void Execute(ConfigurableRenderPipeline pipeline);
}