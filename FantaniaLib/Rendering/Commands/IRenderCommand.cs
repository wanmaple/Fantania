namespace FantaniaLib;

public enum RenderCommandCategory
{
    Setup,
    Draw,
    EndFrame,
}

public interface IRenderCommand
{
    RenderCommandCategory Category { get; }

    void Execute(IRenderDevice device, RenderContext context);
}

public abstract class SetupCommand : IRenderCommand
{
    public RenderCommandCategory Category => RenderCommandCategory.Setup;

    public abstract void Execute(IRenderDevice device, RenderContext context);
}

public abstract class DrawCommand : IRenderCommand
{
    public RenderCommandCategory Category => RenderCommandCategory.Draw;

    public abstract void Execute(IRenderDevice device, RenderContext context);
}

public class EndFrameCommand : IRenderCommand
{
    public RenderCommandCategory Category => RenderCommandCategory.EndFrame;

    public void Execute(IRenderDevice device, RenderContext context)
    {
    }
}