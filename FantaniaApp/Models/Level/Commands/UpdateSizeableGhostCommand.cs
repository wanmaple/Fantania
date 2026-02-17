using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class UpdateSizeableGhostCommand : LevelEntityCommand
{
    public Vector2Int Position { get; set; }
    public Vector2Int Size { get; set; }
    
    public UpdateSizeableGhostCommand(Vector2Int position, Vector2Int size)
    {
        Position = position;
        Size = size;
    }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        if (context.GhostEntity is ISizeableEntity sizeable)
        {
            sizeable.Position = Position;
            sizeable.Size = Size;
            UpdateEntity(context.GhostEntity, context, pipeline);
        }
    }
}