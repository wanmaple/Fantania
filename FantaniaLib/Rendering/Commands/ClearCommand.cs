using System.Numerics;

namespace FantaniaLib;

public class ClearCommand : SetupCommand
{
    public Vector4 ClearColor { get; private set; }
    public int ClearMask { get; private set; }

    public ClearCommand(Vector4 clearColor, int clearMask = GLConstants.GL_COLOR_BUFFER_BIT | GLConstants.GL_DEPTH_BUFFER_BIT)
    {
        ClearColor = clearColor;
        ClearMask = clearMask;
    }

    public override void Execute(IRenderDevice device, RenderContext context)
    {
        device.ClearColor(ClearColor);
        device.ClearBufferBits(ClearMask);
    }
}