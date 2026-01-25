using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class SetupGhostEntityCommand : LevelEntityCommand
{
    public EntitySetups SetupType { get; private set; }

    public SetupGhostEntityCommand(EntitySetups setupType)
    {
        SetupType = setupType;
    }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        UserPlacement placement = context.Workspace.PlacementModule.ActivePlacement!;
        switch (SetupType)
        {
            case EntitySetups.Add:
                {
                    context.GhostEntity = LevelEntity.BuildFromPlacement(placement);
                    context.GhostEntity.Position = context.Workspace.EditorModule.MouseWorldPosition;
                    AddEntity(context.GhostEntity, context, pipeline);
                }
                break;
            case EntitySetups.Remove:
                {
                    RemoveEntity(context.GhostEntity!, context);
                    context.GhostEntity = null;
                }
                break;
        }
    }
}