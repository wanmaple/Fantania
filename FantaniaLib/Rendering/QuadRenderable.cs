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
    private int _depth = 0;
    public int Depth
    {
        get { return _depth; }
        set
        {
            if (_depth != value)
            {
                _depth = value;
                UpdateVertices();
            }
        }
    }
    public int EntityOrder { get; set; }
    public int LocalOrder { get; set; }
    public int NodeIndex { get; set; } = -1;
    public Mesh Mesh => _mesh;
    public RenderMaterial Material => _material!;
    public Rectf BoundingBox => _aabb;
    public Vector2 Anchor { get; private set; } = Vector2.Zero;
    public Vector2 Size { get; private set; } = Vector2.Zero;
    public Rectf Tiling { get; private set; } = Rectf.Zero;
    public Rectf Tiling2 { get; private set; } = Rectf.Zero;
    public Vector4 VertexColor { get; private set; } = Vector4.One;

    public QuadRenderable(RenderInfo info, RenderMaterial material, IReadOnlyDictionary<string, object?> customArgs)
    {
        _mesh = MeshBuilder.CreateStandardQuad(info.Size);
        Stage = info.Stage;
        Depth = info.Depth;
        EntityOrder = info.EntityOrder;
        LocalOrder = info.LocalOrder;
        Anchor = info.Anchor;
        Size = info.Size;
        Tiling = info.Tiling;
        Tiling2 = info.Tiling2;
        VertexColor = info.Color;
        _material = material;
        _transform = info.Transform;
        UpdateVertices();
        CalculateBounds(Transform);
    }

    void UpdateVertices()
    {
        for (int i = 0; i < 4; i++)
        {
            VertexStandard vert = Mesh.GetVerticeAt<VertexStandard>(i);
            vert.Position = new Vector3(Transform * (QUAD_VERTICES[i] * Size), Depth);
            vert.UV = Tiling.TopLeft + QUAD_VERTICES[i] * Tiling.Size;
            vert.Color = VertexColor;
            vert.UV2 = Tiling2.TopLeft + QUAD_VERTICES[i] * Tiling2.Size;
            Mesh.SetVerticeAt(i, vert);
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

    Mesh _mesh;
    RenderMaterial _material;
    Rectf _aabb;

    static readonly Vector2[] QUAD_VERTICES = [
        Vector2.Zero,
        Vector2.UnitX,
        Vector2.One,
        Vector2.UnitY,
    ];
}