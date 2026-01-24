using System.Numerics;
using FantaniaLib;

namespace Fantania.Views;

public class ViewEditorMode : ILevelEditorMode
{
    public void OnEnter(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnExit(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnKeyDown(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnKeyUp(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnMouseMoved(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (e.MouseState.IsMiddleButtonPressed)
        {
            Vector2 movement = e.MouseState.Movement.ToVector2();
            Vector2 newPos = e.MouseState.Position.ToVector2();
            Vector2 oldPos = newPos - movement;
            Vector2 newWorldPos = context.CanvasToWorld(newPos);
            Vector2 oldWorldPos = context.CanvasToWorld(oldPos);
            Vector2 movementWorld = newWorldPos - oldWorldPos;
            context.Camera.Translate(-movementWorld);
            context.Workspace.UserTemporary.CameraPosition = context.Camera.Position;
        }
    }

    public void OnMousePressed(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnMouseReleased(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnMouseWheelChanged(LevelEditorContext context, ControlInputEventArgs e)
    {
    }
}