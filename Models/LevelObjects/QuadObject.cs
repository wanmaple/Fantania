using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Avalonia;

namespace Fantania.Models;

public class QuadObject : LevelObject
{
    public QuadObject()
    {
    }

    public QuadObject([NotNull] DrawTemplate template)
    : base(null)
    {
        _template = template;
    }

    public override void OnReadyAdding(Level lv)
    {
        _addingSelection = ObjectPool<Selection>.Get(this);
        _addingSelection.VertexColor = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        lv.AddObject(_addingSelection, false);
        lv.AddObject(this, false);
    }

    public override void OnCancelAdding(Level lv)
    {
        lv.RemoveObject(_addingSelection);
        lv.RemoveObject(this);
        ObjectPool<Selection>.Return(_addingSelection);
    }

    public override void OnPlaceFromAdding(Level lv)
    {
        lv.RemoveObject(_addingSelection);
        ObjectPool<Selection>.Return(_addingSelection);
    }

    protected override void CalculateBounds(Matrix3x3 transform)
    {
        // 顺便把BVH用的包围盒也一起更新了，为了方便就用OpenGL坐标系下的包围盒。
        Vector2 pt1 = transform * Vector2.Zero;
        Vector2 pt2 = transform * new Vector2((float)Size.X, 0.0f);
        Vector2 pt3 = transform * Size.ToVector2();
        Vector2 pt4 = transform * new Vector2(0.0f, (float)Size.Y);
        float minX = MathF.Min(pt1.X, MathF.Min(pt2.X, MathF.Min(pt3.X, pt4.X)));
        float maxX = MathF.Max(pt1.X, MathF.Max(pt2.X, MathF.Max(pt3.X, pt4.X)));
        float minY = MathF.Min(pt1.Y, MathF.Min(pt2.Y, MathF.Min(pt3.Y, pt4.Y)));
        float maxY = MathF.Max(pt1.Y, MathF.Max(pt2.Y, MathF.Max(pt3.Y, pt4.Y)));
        _bounds = new Rect(new Point(minX, minY), new Point(maxX, maxY));
        _boundVerts[0] = pt1;
        _boundVerts[1] = pt2;
        _boundVerts[2] = pt3;
        _boundVerts[3] = pt4;
    }

    public override bool PointTestInExactBounds(Point pt)
    {
        // 这是额外的判断条件，所以如果没有旋转就默认在范围内
        if (_rotation == 0.0)
            return true;
        return MathHelper.IsPointInsideConvexQuadrilateral(pt.ToVector2(), _boundVerts);
    }

    public override string ToString()
    {
        return "<Quad> " + base.ToString();
    }

    Vector2[] _boundVerts = new Vector2[4];
    Selection _addingSelection;
}