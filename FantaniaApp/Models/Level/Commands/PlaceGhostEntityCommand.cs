using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class PlaceGhostEntityCommand : LevelEntityCommand
{
    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        if (context.GhostEntity != null)
        {
            context.Workspace.LevelModule.PlaceEntity(context.GhostEntity);
            // 继续添加
            UserPlacement placement = context.Workspace.PlacementModule.ActivePlacement!;
            context.GhostEntity = LevelEntity.BuildFromPlacement(context.Workspace, placement);
            context.GhostEntity.Position = context.Workspace.EditorModule.MouseWorldPosition;
            context.GhostEntity.Order = context.Workspace.LevelModule.CurrentLevel!.NewOrder();
            AddEntity(context.GhostEntity, context, pipeline);
        }
    }
}