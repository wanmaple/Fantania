namespace FantaniaLib;

public class SetupStateCommand : IRenderCommand
{
    public RenderState State { get; private set; }

    public SetupStateCommand(RenderState state)
    {
        State = state;
    }

    public void Execute(ConfigurableRenderPipeline pipeline)
    {
        pipeline.Device.ApplyRenderState(State);
    }
}