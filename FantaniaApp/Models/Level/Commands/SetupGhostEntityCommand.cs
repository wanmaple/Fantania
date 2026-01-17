using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class SetupGhostEntityCommand : ICanvasCommand
{
    public enum GhostSetups
    {
        Add,
        Remove,
        Confirm,
    }

    public GhostSetups SetupType { get; private set; }

    public SetupGhostEntityCommand(GhostSetups setupType)
    {
        SetupType = setupType;
    }

    public void Execute(LevelRenderContext context, ConfigurableRenderPipeline pipeline)
    {
        UserPlacement placement = context.Workspace.PlacementModule.ActivePlacement!;
        Level level = context.Workspace.LevelModule.CurrentLevel!;
        switch (SetupType)
        {
            case GhostSetups.Add:
                context.GhostEntity = LevelEntity.BuildFromPlacement(placement);
                context.GhostEntity.GetRenderables(context.Workspace);
                break;
            case GhostSetups.Remove:
                context.GhostEntity = null;
                break;
        }
    }
}