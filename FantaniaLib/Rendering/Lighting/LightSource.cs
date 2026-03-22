using System.Numerics;

namespace FantaniaLib;

public struct LightSourceInfo
{
    public TextureDefinition LightTexture;
    public float Radius;
    public float Intensity;
    public Vector4 Color;
    public int LightingLayer;
    public Vector3 Position;
}

public class LightSource : RenderableBase
{
    private Matrix3x3 _transform = Matrix3x3.Identity;
    public override Matrix3x3 Transform
    {
        get => _transform;
        set
        {
            if (_transform != value)
            {
                _transform = value;
                _lightInfo.Position = new Vector3(_transform.GetTranslation(), Depth);
                UpdateVertices();
                CalculateBounds(_transform);
            }
        }
    }
    private int _depth = 0;
    public override int Depth
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
    public override Mesh Mesh => _mesh;
    public override RenderMaterial Material => _material!;
    public override Rectf BoundingBox => _aabb;

    public LightSourceInfo LightInfo => _lightInfo;

    public LightSource(RenderInfo info, RenderMaterial material, IReadOnlyDictionary<string, object?> customArgs)
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
        Transform = info.Transform;
        SetupLightInfo(customArgs);
        UpdateVertices();
        CalculateBounds(Transform);
    }

    void SetupLightInfo(IReadOnlyDictionary<string, object?> customArgs)
    {
        if (customArgs.TryGetValue("lightTexture", out object? lightTexObj) && lightTexObj is TextureDefinition lightTex)
        {
            _lightInfo.LightTexture = lightTex;
        }
        else
        {
            _lightInfo.LightTexture = TextureDefinition.None;
        }
        if (customArgs.TryGetValue("radius", out object? radiusObj) && radiusObj is int radius)
        {
            _lightInfo.Radius = radius;
        }
        else
        {
            _lightInfo.Radius = 0.0f;
        }
        if (customArgs.TryGetValue("color", out object? colorObj) && colorObj is Vector4 color)
        {
            _lightInfo.Color = color;
        }
        else
        {
            _lightInfo.Color = Vector4.One;
        }
        if (customArgs.TryGetValue("lightingLayer", out object? layerObj) && layerObj is int layer)
        {
            _lightInfo.LightingLayer = layer;
        }
        else
        {
            _lightInfo.LightingLayer = 0;
        }
        if (customArgs.TryGetValue("intensity", out object? intensityObj) && intensityObj is float intensity)
        {
            _lightInfo.Intensity = intensity;
        }
        else
        {
            _lightInfo.Intensity = 1.0f;
        }
        _lightInfo.Position = new Vector3(Transform.GetTranslation(), Depth);
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
            Vector2 scale = Transform.GetScale();
            vert.RotationScale = new Vector4(Transform.GetRotation(), scale.X, scale.Y, 0.0f);
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
        Vector2 center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        _aabb = new Rectf(center.X - _lightInfo.Radius, center.Y - _lightInfo.Radius, _lightInfo.Radius * 2.0f, _lightInfo.Radius * 2.0f);
        OnPropertyChanged(nameof(BoundingBox));
    }

    Mesh _mesh;
    RenderMaterial _material;
    Rectf _aabb;
    LightSourceInfo _lightInfo = new LightSourceInfo();

    static readonly Vector2[] QUAD_VERTICES = [
        Vector2.Zero,
        Vector2.UnitX,
        Vector2.One,
        Vector2.UnitY,
    ];
}