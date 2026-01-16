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
        _aabb = new Rectf(0.0f, 0.0f, info.Size.X, info.Size.Y);
    }

    void UpdateVertices(RenderInfo info)
    {
        for (int i = 0; i < 4; i++)
        {
            VertexStandard vert = _mesh.GetVerticeAt<VertexStandard>(i);
            vert.Position = new Vector3(Transform * vert.Position.ToVector2(), Depth);
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

    Mesh _mesh;
    RenderMaterial _material;
    Rectf _aabb;
}