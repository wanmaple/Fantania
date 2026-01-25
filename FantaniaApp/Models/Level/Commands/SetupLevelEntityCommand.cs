using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class SetupLevelEntityCommand : LevelEntityCommand
{
    public LevelEntity Entity { get; private set; }
    public EntitySetups SetupType { get; private set; }

    public SetupLevelEntityCommand(LevelEntity entity, EntitySetups setupType)
    {
        Entity = entity;
        SetupType = setupType;
    }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        switch (SetupType)
        {
            case EntitySetups.Add:
                AddEntity(Entity, context, pipeline);
                break;
            case EntitySetups.Remove:
                RemoveEntity(Entity, context);
                break;
        }
    }
}