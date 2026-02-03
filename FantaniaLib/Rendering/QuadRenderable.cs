using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class QuadRenderable : ObservableObject, IRenderable
{
    public string Stage { get; private set; } = string.Empty;
    private Matrix3x3 _transform = Matrix3x3.Identity;
    public Matrix3x3 Transform
    {
        get { return _transform; }
        set
        {
            if (_transform != value)
            {
                _transform = value;
                UpdateVertices();
                CalculateBounds(Transform);
            }
        }
    }
    public int Depth { get; set; }
    public int EntityOrder { get; set; }
    public int LocalOrder { get; set; }
    public int NodeIndex { get; set; } = -1;
    public Mesh Mesh => _mesh;
    public RenderMaterial Material => _material;
    public Rectf BoundingBox => _aabb;
    public Vector2 Anchor { get; private set; } = Vector2.Zero;
    public Vector2 Size { get; private set; } = Vector2.Zero;
    public Vector4 VertexColor { get; private set; } = Vector4.One;

    public QuadRenderable(RenderInfo info, RenderMaterial material)
    {
        _mesh = MeshBuilder.CreateStandardQuad(info.Size);
        Stage = info.Stage;
        Depth = info.Depth;
        EntityOrder = info.EntityOrder;
        LocalOrder = info.LocalOrder;
        Anchor = info.Anchor;
        Size = info.Size;
        VertexColor = info.Color;
        _material = material;
        _transform = info.Transform;
        UpdateVertices();
        CalculateBounds(Transform);
    }

    private QuadRenderable(QuadRenderable other)
    {
        Stage = other.Stage;
        _transform = other._transform;
        Depth = other.Depth;
        EntityOrder = other.EntityOrder;
        LocalOrder = other.LocalOrder;
        Anchor = other.Anchor;
        Size = other.Size;
        VertexColor = other.VertexColor;
        _material = other._material.Clone();
        _mesh = other._mesh;
        _aabb = other._aabb;
        _exactVerts = other._exactVerts;
    }

    void UpdateVertices()
    {
        for (int i = 0; i < 4; i++)
        {
            VertexStandard vert = _mesh.GetVerticeAt<VertexStandard>(i);
            vert.Position = new Vector3(Transform * (QUAD_VERTICES[i] * Size), Depth);
            vert.Color = VertexColor;
            _mesh.SetVerticeAt(i, vert);
        }
    }

    protected void CalculateBounds(Matrix3x3 transform)
    {
        Vector2 pt1 = transform * Vector2.Zero;
        Vector2 pt2 = transform * new Vector2(Size.X, 0.0f);
        Vector2 pt3 = transform * Size;
        Vector2 pt4 = transform * new Vector2(0.0f, Size.Y);
        float minX = MathF.Min(pt1.X, MathF.Min(pt2.X, MathF.Min(pt3.X, pt4.X)));
        float maxX = MathF.Max(pt1.X, MathF.Max(pt2.X, MathF.Max(pt3.X, pt4.X)));
        float minY = MathF.Min(pt1.Y, MathF.Min(pt2.Y, MathF.Min(pt3.Y, pt4.Y)));
        float maxY = MathF.Max(pt1.Y, MathF.Max(pt2.Y, MathF.Max(pt3.Y, pt4.Y)));
        _aabb = new Rectf(minX, minY, maxX - minX, maxY - minY);
        OnPropertyChanged(nameof(BoundingBox));
    }

    public IRenderable Clone()
    {
        return new QuadRenderable(this);
    }

    Mesh _mesh;
    RenderMaterial _material;
    Rectf _aabb;
    Vector2[] _exactVerts = new Vector2[4];

    static readonly Vector2[] QUAD_VERTICES = [
        Vector2.Zero,
        Vector2.UnitX,
        Vector2.One,
        Vector2.UnitY,
    ];
}