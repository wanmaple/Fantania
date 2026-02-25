using System;
using System.Collections.Generic;
using System.Numerics;
using FantaniaLib;

namespace Fantania.Views;

public enum ResizeHandleTypes
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
}

public struct ResizeStartInfo
{
    public Vector2Int Position;
    public Vector2Int Size;
    public Vector2Int UnitSize;
}

public class ResizeContext
{
    public bool IsActive => _activeHandle != ResizeHandleTypes.None;

    public bool Start(IWorkspace workspace, IReadOnlyList<ISelectableItem> selections, Vector2 worldPos, Vector2 canvasPos, ResizeHandleTypes handle)
    {
        _activeHandle = handle;
        _startWorld = worldPos;
        _startEntities.Clear();
        foreach (var sel in selections)
        {
            ISizeableEntity sizeable = (ISizeableEntity)sel;
            _startEntities[sizeable] = new ResizeStartInfo
            {
                Position = sizeable.Position,
                Size = sizeable.Size,
                UnitSize = sizeable.GetUnitSize(workspace),
            };
        }
        return true;
    }

    public void Update(LevelSpaceContext context, Vector2 worldPos)
    {
        if (!IsActive)
            return;

        Vector2 worldDelta = worldPos - _startWorld;
        foreach (var (entity, info) in _startEntities)
        {
            Vector2Int unit = new Vector2Int(info.UnitSize.X > 0 ? info.UnitSize.X : 1, info.UnitSize.Y > 0 ? info.UnitSize.Y : 1);
            int deltaUnitX = MathHelper.RoundToInt(worldDelta.X / unit.X);
            int deltaUnitY = MathHelper.RoundToInt(worldDelta.Y / unit.Y);

            Vector2Int newPosition = info.Position;
            Vector2Int newSize = info.Size;

            if ((_activeHandle & ResizeHandleTypes.Left) != 0)
            {
                newSize.X = info.Size.X - deltaUnitX;
                if (newSize.X < 1)
                    newSize.X = 1;
                newPosition.X = info.Position.X + (info.Size.X - newSize.X) * unit.X;
            }
            else if ((_activeHandle & ResizeHandleTypes.Right) != 0)
            {
                newSize.X = info.Size.X + deltaUnitX;
                if (newSize.X < 1)
                    newSize.X = 1;
            }

            if ((_activeHandle & ResizeHandleTypes.Top) != 0)
            {
                newSize.Y = info.Size.Y - deltaUnitY;
                if (newSize.Y < 1)
                    newSize.Y = 1;
                newPosition.Y = info.Position.Y + (info.Size.Y - newSize.Y) * unit.Y;
            }
            else if ((_activeHandle & ResizeHandleTypes.Bottom) != 0)
            {
                newSize.Y = info.Size.Y + deltaUnitY;
                if (newSize.Y < 1)
                    newSize.Y = 1;
            }

            if (entity.Position != newPosition)
                entity.Position = newPosition;
            if (entity.Size != newSize)
                entity.Size = newSize;
        }
        context.Workspace.EditorModule.Notify();
    }

    public void End()
    {
        _activeHandle = ResizeHandleTypes.None;
        _startWorld = Vector2.Zero;
        _startEntities.Clear();
    }

    ResizeHandleTypes _activeHandle = ResizeHandleTypes.None;
    Vector2 _startWorld = Vector2.Zero;
    Dictionary<ISizeableEntity, ResizeStartInfo> _startEntities = new Dictionary<ISizeableEntity, ResizeStartInfo>(8);
}
