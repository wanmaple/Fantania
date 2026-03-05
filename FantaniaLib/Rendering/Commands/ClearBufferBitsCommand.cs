namespace FantaniaLib;

public class ClearBufferBitsCommand : IRenderCommand
{
    public BufferBits Bits { get; set; }

    public ClearBufferBitsCommand(BufferBits bits)
    {
        Bits = bits;
    }

    public void Execute(ConfigurableRenderPipeline pipeline)
    {
        pipeline.Device.ClearBufferBits(Bits);
    }
}