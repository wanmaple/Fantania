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
        var activePlacement = context.Workspace.PlacementModule.ActivePlacement;
        if (activePlacement != null)
        {
            if (activePlacement.Template.PlacementType == PlacementTypes.Tiled && e.MouseState.IsLeftButtonPressed)
            {
                Vector2Int tileSize = activePlacement.Template.TileSize;
                Vector2Int gridPos = worldPos.ToGridSpace(context.EditConfig.GridAlign);
                _rangeBox.Current = gridPos.ToVector2();
                int width = MathHelper.RoundToInt(_rangeBox.Width / tileSize.X);
                int height = MathHelper.RoundToInt(_rangeBox.Height / tileSize.Y);
                context.AddCommand(new UpdateGhostTiledEntityCommand(new Vector2Int((int)_rangeBox.Left, (int)_rangeBox.Top), new Vector2Int(width, height)));
            }
            else
                context.AddCommand(UpdateGhostEntityCommand.UpdatePosition(worldPos));
        }
    }

    public void OnMousePressed(LevelEditorContext context, ControlInputEventArgs e)
    {
        var activePlacement = context.Workspace.PlacementModule.ActivePlacement;
        if (activePlacement != null && activePlacement.Template.PlacementType == PlacementTypes.Tiled)
        {
            if (e.MouseState.IsLeftButtonPressed)
            {
                Vector2Int gridPos = context.CanvasToWorld(e.MouseState.Position.ToVector2()).ToGridSpace(context.EditConfig.GridAlign);
                _rangeBox.Origin = gridPos.ToVector2();
                _rangeBox.Current = _rangeBox.Origin;
            }
        }
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

    SelectionBox _rangeBox = new SelectionBox();
}