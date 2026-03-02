using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public class SingleNodeEntity : LevelEntity, ISelectableItem
{
    public Rectf BoundingBox => _aabb;
    public Color SelectionColor => Colors.Yellow;
    public Vector2 Anchor => (Position.ToVector2() - _aabb.TopLeft) / _aabb.Size;
    public Vector2 WorldPosition => Position.ToVector2();
    public override int NodeCount => 1;
    public int Depth => RealDepth;
    public int EntityOrder => Order;
    public int LocalOrder => 0;

    internal SingleNodeEntity()
    {}

    public override IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        var fields = (List<IEditableField>)base.GetEditableFields(workspace);
        if (!CanTranslate(workspace))
        {
            fields.RemoveAll(f => f.FieldName == nameof(Position));
        }
        if (!CanRotate(workspace))
        {
            fields.RemoveAll(f => f.FieldName == nameof(Rotation));
        }
        if (!CanScale(workspace))
        {
            fields.RemoveAll(f => f.FieldName == nameof(Scale));
        }
        return fields;
    }

    public override void GetLocalNodeAt(IWorkspace workspace, int index, out IReadOnlyList<LocalRenderInfo> locals)
    {
        UserPlacement placement = GetReferencedPlacement(workspace);
        locals = placement.GetLocalNodeAt(index, 1);
    }

    public override void OnAddSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index, Rectf bound)
    {
        _localBound = bound;
        Matrix3x3 transform = TransformAt(0);
        CalculateBounds(transform, _localBound);
        bvh.AddItem(this);
        OnPropertyChanged(nameof(BoundingBox));
    }

    public override void OnRemoveSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh)
    {
        bvh.RemoveItem(this);
    }

    public override void OnUpdateSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index)
    {
        Matrix3x3 transform = TransformAt(0);
        CalculateBounds(transform, _localBound);
        bvh.UpdateItem(this);
        OnPropertyChanged(nameof(BoundingBox));
    }

    void CalculateBounds(Matrix3x3 transform, Rectf bound)
    {
        Vector2 pt1 = transform * bound.TopLeft;
        Vector2 pt2 = transform * bound.TopRight;
        Vector2 pt3 = transform * bound.BottomRight;
        Vector2 pt4 = transform * bound.BottomLeft;
        float minX = MathF.Min(pt1.X, MathF.Min(pt2.X, MathF.Min(pt3.X, pt4.X)));
        float maxX = MathF.Max(pt1.X, MathF.Max(pt2.X, MathF.Max(pt3.X, pt4.X)));
        float minY = MathF.Min(pt1.Y, MathF.Min(pt2.Y, MathF.Min(pt3.Y, pt4.Y)));
        float maxY = MathF.Max(pt1.Y, MathF.Max(pt2.Y, MathF.Max(pt3.Y, pt4.Y)));
        _aabb = new Rectf(minX, minY, maxX - minX, maxY - minY);
        // _exactVerts[0] = pt1;
        // _exactVerts[1] = pt2;
        // _exactVerts[2] = pt3;
        // _exactVerts[3] = pt4;
        OnPropertyChanged(nameof(Anchor));
    }

    public bool PointTest(Vector2 pt)
    {
        // if (Rotation != 0.0f)
        //     return MathHelper.IsPointInsideConvexQuadrilateral(pt, _exactVerts);
        return true;
    }

    public virtual bool CanRotate(IWorkspace workspace)
    {
        return GetReferencedPlacement(workspace).Template.CanRotate(0);
    }

    public virtual bool CanScale(IWorkspace workspace)
    {
        return GetReferencedPlacement(workspace).Template.CanScale(0);
    }

    public virtual bool CanTranslate(IWorkspace workspace)
    {
        return GetReferencedPlacement(workspace).Template.CanTranslate(0);
    }

    public override Matrix3x3 TransformAt(int index)
    {
        return SelfTransform;
    }

    Rectf _aabb, _localBound;
    // Vector2[] _exactVerts = new Vector2[4];
}