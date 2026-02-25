using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class UpdateResizeCommand : ICanvasCommand
{
    public Vector2 WorldPosition { get; private set; }

    public UpdateResizeCommand(Vector2 worldPosition)
    {
        WorldPosition = worldPosition;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        context.ResizeContext.Update(context, WorldPosition);
    }
}
