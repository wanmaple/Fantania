using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class StartResizeCommand : ICanvasCommand
{
    public Vector2 WorldPosition { get; private set; }
    public Vector2 CanvasPosition { get; private set; }
    public ResizeHandleTypes Handle { get; private set; }

    public StartResizeCommand(Vector2 worldPosition, Vector2 canvasPosition, ResizeHandleTypes handle)
    {
        WorldPosition = worldPosition;
        CanvasPosition = canvasPosition;
        Handle = handle;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        var selections = context.Workspace.EditorModule.SelectedObjects;
        context.ResizeContext.Start(context.Workspace, selections, WorldPosition, CanvasPosition, Handle);
    }
}
