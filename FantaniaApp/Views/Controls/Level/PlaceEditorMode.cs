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
        context.AddCommand(new ClearSnapsCommand());
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
            if (activePlacement.TemplateAs<PlacementTemplate>().IsSizeable && e.MouseState.IsLeftButtonPressed)
            {
                Vector2Int tileSize = activePlacement.TemplateAs<PlacementTemplate>().TileSize;
                Vector2Int gridPos = worldPos.ToGridSpace(context.EditConfig.GridAlign);
                _rangeBox.Current = gridPos.ToVector2();
                int width = MathHelper.RoundToInt(_rangeBox.Width / tileSize.X);
                int height = MathHelper.RoundToInt(_rangeBox.Height / tileSize.Y);
                int left = _rangeBox.Current.X < _rangeBox.Origin.X ? (int)_rangeBox.Origin.X - width * tileSize.X : (int)_rangeBox.Origin.X;
                int top = _rangeBox.Current.Y < _rangeBox.Origin.Y ? (int)_rangeBox.Origin.Y - height * tileSize.Y : (int)_rangeBox.Origin.Y;
                context.AddCommand(new UpdateSizeableGhostCommand(new Vector2Int(left, top), new Vector2Int(width, height)));
            }
            else
                context.AddCommand(UpdateGhostEntityCommand.UpdatePosition(worldPos));
            if (!e.MouseState.IsLeftButtonPressed)
                context.AddCommand(new UpdateSnapsCommand(worldPos));
        }
    }

    public void OnMousePressed(LevelEditorContext context, ControlInputEventArgs e)
    {
        var activePlacement = context.Workspace.PlacementModule.ActivePlacement;
        if (activePlacement != null && activePlacement.TemplateAs<PlacementTemplate>().IsSizeable)
        {
            if (e.MouseState.IsLeftButtonPressed)
            {
                Vector2Int origin;
                if (context.Workspace.EditorModule.SnapPoints.Count > 0)
                    origin = context.Workspace.EditorModule.SnapPoints[0].Position.ToVector2i();
                else
                    origin = context.CanvasToWorld(e.MouseState.Position.ToVector2()).ToGridSpace(context.EditConfig.GridAlign);
                _rangeBox.Origin = origin.ToVector2();
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