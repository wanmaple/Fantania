using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class UpdateLevelEntityCommand : LevelEntityCommand
{
    public LevelEntity Entity { get; private set; }

    public UpdateLevelEntityCommand(LevelEntity entity)
    {
        Entity = entity;
    }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        UpdateEntity(Entity, context, pipeline);
        context.Workspace.EditorModule.Notify();
    }
}