using System.Numerics;
using Fantania.Models;
using FantaniaLib;

namespace Fantania.Views;

public class PlaceEditorMode : ILevelEditorMode
{
    public void OnEnter(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (context.Workspace.PlacementModule.ActivePlacement != null)
            context.AddCommand(new SetupGhostEntityCommand(EntitySetups.Add));
    }

    public void OnExit(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (context.Workspace.PlacementModule.ActivePlacement != null)
            context.AddCommand(new SetupGhostEntityCommand(EntitySetups.Remove));
    }

    public void OnKeyDown(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnKeyUp(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnMouseMoved(LevelEditorContext context, ControlInputEventArgs e)
    {
        Vector2 worldPos = context.CanvasToWorld(e.MouseState.Position.ToVector2());
        if (context.Workspace.PlacementModule.ActivePlacement != null)
            context.AddCommand(UpdateGhostEntityCommand.UpdatePosition(worldPos));
    }

    public void OnMousePressed(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnMouseReleased(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (e.MouseState.IsLeftButtonJustReleased)
        {
            context.AddCommand(new PlaceGhostEntityCommand());
        }
    }

    public void OnMouseWheelChanged(LevelEditorContext context, ControlInputEventArgs e)
    {
    }
}