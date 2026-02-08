using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public class TiledEntity : LevelEntity, ISelectableItem
{
    public override int NodeCount => 1;
    public int Depth => RealDepth;

    public Color SelectionColor => Colors.Orange;
    public Vector2 Anchor => (Position.ToVector2() - _aabb.TopLeft) / _aabb.Size;
    public Vector2 WorldPosition => Position.ToVector2();
    public int EntityOrder => Order;
    public int LocalOrder => 0;

    public Rectf BoundingBox => _aabb;

    internal TiledEntity()
    {
        for (int i = 0; i < _serializableFields.Count; )
        {
            if (_serializableFields[i].FieldName == nameof(Rotation) || _serializableFields[i].FieldName == nameof(Scale))
            {
                _serializableFields.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    public bool PointTest(Vector2 pt)
    {
        return true;
    }

    public bool CanTranslate(IWorkspace workspace)
    {
        return GetReferencedPlacement(workspace).Template.CanTranslate(0);
    }

    public bool CanRotate(IWorkspace workspace)
    {
        return false;
    }

    public bool CanScale(IWorkspace workspace)
    {
        return false;
    }

    public override Matrix3x3 TransformAt(int index)
    {
        throw new NotImplementedException();
    }

    public override void GetLocalNodeAt(IWorkspace workspace, int index, out IReadOnlyList<LocalRenderInfo> locals)
    {
        throw new NotImplementedException();
    }

    public override void OnAddSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index, Rectf bound)
    {
        throw new NotImplementedException();
    }

    public override void OnRemoveSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh)
    {
        throw new NotImplementedException();
    }

    public override void OnUpdateSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index)
    {
        throw new NotImplementedException();
    }

    Rectf _aabb;
}