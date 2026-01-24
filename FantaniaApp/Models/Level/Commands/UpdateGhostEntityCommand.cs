using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class UpdateGhostEntityCommand : LevelEntityCommand
{
    public enum GhostUpdates
    {
        Position,
        Rotation,
        Scale,
    }

    public GhostUpdates UpdateType { get; set; } = GhostUpdates.Position;

    public static UpdateGhostEntityCommand UpdatePosition(Vector2 pos)
    {
        return new UpdateGhostEntityCommand
        {
            UpdateType = GhostUpdates.Position,
            _param = pos,
        };
    }

    public static UpdateGhostEntityCommand UpdateRotation(float rot)
    {
        return new UpdateGhostEntityCommand
        {
            UpdateType = GhostUpdates.Rotation,
            _param = new Vector2(rot, rot),
        };
    }

    public static UpdateGhostEntityCommand UpdateScale(Vector2 scale)
    {
        return new UpdateGhostEntityCommand
        {
            UpdateType = GhostUpdates.Scale,
            _param = scale,
        };
    }

    private UpdateGhostEntityCommand()
    { }

    public override void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        switch (UpdateType)
        {
            case GhostUpdates.Position:
                context.GhostEntity!.Position = new Vector2Int((int)_param.X, (int)_param.Y);
                break;
            case GhostUpdates.Rotation:
                context.GhostEntity!.Rotation += _param.X;
                break;
            case GhostUpdates.Scale:
                break;
        }
        UpdateEntity(context.GhostEntity!, context, pipeline);
    }

    Vector2 _param = Vector2.Zero;
}