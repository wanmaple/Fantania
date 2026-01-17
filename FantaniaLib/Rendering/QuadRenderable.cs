using System.Numerics;

namespace FantaniaLib;

public class QuadRenderable : IRenderable
{
    public string Stage { get; private set; }
    public Matrix3x3 Transform { get; private set; }
    public int Depth { get; private set; }
    public Mesh Mesh => _mesh;
    public RenderMaterial Material => _material;
    public Rectf BoundingBox => _aabb;

    public QuadRenderable(RenderInfo info, RenderMaterial material)
    {
        _mesh = MeshBuilder.CreateStandardQuad(info.Size);
        Stage = info.Stage;
        Depth = info.Depth;
        Transform = BuildTransform(info);
        _material = material;
        UpdateVertices(info);
        CalculateBounds(Transform, info.Size);
    }

    void UpdateVertices(RenderInfo info)
    {
        for (int i = 0; i < 4; i++)
        {
            VertexStandard vert = _mesh.GetVerticeAt<VertexStandard>(i);
            vert.Position = new Vector3(Transform * vert.Position.XY(), Depth);
            vert.Color = info.Color;
            _mesh.SetVerticeAt(i, vert);
        }
    }

    protected Matrix3x3 BuildTransform(RenderInfo info)
    {
        // Transform矩阵可以分解成四部分，首先进行Anchor相关的平移，然后进行缩放，然后进行旋转，最后进行世界位置的平移。
        Matrix3x3 mat = Matrix3x3.Identity;
        Vector2 anchor = info.Anchor;
        if (anchor != Vector2.Zero)
        {
            mat = Matrix3x3.CreateTranslation(new Vector2(-anchor.X * info.Size.X, -anchor.Y * info.Size.Y));
        }
        if (info.Scale != Vector2.One)
        {
            mat = Matrix3x3.CreateScale(info.Scale) * mat;
        }
        if (info.Rotation != 0.0f)
        {
            mat = Matrix3x3.CreateRotation(info.Rotation) * mat;
        }
        if (info.Position != Vector2.Zero)
        {
            mat = Matrix3x3.CreateTranslation(info.Position) * mat;
        }
        return mat;
    }

    protected void CalculateBounds(Matrix3x3 transform, Vector2 size)
    {
        // 顺便把BVH用的包围盒也一起更新了，为了方便就用OpenGL坐标系下的包围盒。
        Vector2 pt1 = transform * Vector2.Zero;
        Vector2 pt2 = transform * new Vector2(size.X, 0.0f);
        Vector2 pt3 = transform * size;
        Vector2 pt4 = transform * new Vector2(0.0f, size.Y);
        float minX = MathF.Min(pt1.X, MathF.Min(pt2.X, MathF.Min(pt3.X, pt4.X)));
        float maxX = MathF.Max(pt1.X, MathF.Max(pt2.X, MathF.Max(pt3.X, pt4.X)));
        float minY = MathF.Min(pt1.Y, MathF.Min(pt2.Y, MathF.Min(pt3.Y, pt4.Y)));
        float maxY = MathF.Max(pt1.Y, MathF.Max(pt2.Y, MathF.Max(pt3.Y, pt4.Y)));
        _aabb = new Rectf(minX, minY, maxX - minX, maxY - minY);
        _exactVerts[0] = pt1;
        _exactVerts[1] = pt2;
        _exactVerts[2] = pt3;
        _exactVerts[3] = pt4;
    }

    public bool PointTest(Vector2 pt)
    {
        return MathHelper.IsPointInsideConvexQuadrilateral(pt, _exactVerts);
    }

    Mesh _mesh;
    RenderMaterial _material;
    Rectf _aabb;
    Vector2[] _exactVerts = new Vector2[4];
}