using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class SetupGhostEntityCommand : LevelEntityCommand
{
    public enum GhostSetups
    {
        Add,
        Remove,
    }

    public GhostSetups SetupType { get; private set; }

    public SetupGhostEntityCommand(GhostSetups setupType)
    {
        SetupType = setupType;
    }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        UserPlacement placement = context.Workspace.PlacementModule.ActivePlacement!;
        switch (SetupType)
        {
            case GhostSetups.Add:
                {
                    context.GhostEntity = LevelEntity.BuildFromPlacement(placement);
                    context.Workspace.LogModule.LogOptional("Add Ghost");
                    AddEntity(context.GhostEntity, context, pipeline);
                }
                break;
            case GhostSetups.Remove:
                {
                    RemoveEntity(context.GhostEntity!, context);
                    context.GhostEntity = null;
                    context.Workspace.LogModule.LogOptional("Remove Ghost");
                }
                break;
        }
    }
}