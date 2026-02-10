using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class UpdateGhostTiledEntityCommand : LevelEntityCommand
{
    public Vector2Int Position { get; set; }
    public Vector2Int Size { get; set; }
    
    public UpdateGhostTiledEntityCommand(Vector2Int position, Vector2Int size)
    {
        Position = position;
        Size = size;
    }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        if (context.GhostEntity is TiledEntity tiled)
        {
            tiled.Position = Position;
            tiled.Size = Size;
            UpdateEntity(context.GhostEntity, context, pipeline);
        }
    }
}