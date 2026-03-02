using System.Numerics;

namespace FantaniaLib;

public class ClearColorCommand : IRenderCommand
{
    public Vector4 Color { get; set; } = Vector4.Zero;

    public ClearColorCommand(Vector4 color)
    {
        Color = color;
    }

    public void Execute(ConfigurableRenderPipeline pipeline)
    {
        pipeline.Device.ClearColor(Color);
    }
}