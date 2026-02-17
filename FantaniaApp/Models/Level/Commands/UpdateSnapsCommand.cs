using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class UpdateSnapsCommand : LevelEntityCommand
{
    public Vector2 WorldPosition { get; set; }

    public UpdateSnapsCommand(Vector2 worldPos)
    {
        WorldPosition = worldPos;
    }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        if (context.GhostEntity != null)
        {
            context.GhostEntity.OnUpdateSnaps(context.Workspace, context.SelectableHierarchy, WorldPosition);
        }
    }
}