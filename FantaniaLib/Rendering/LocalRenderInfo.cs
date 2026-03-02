using System.Numerics;

namespace FantaniaLib;

public class LocalRenderInfo
{
    public required string Stage { get; set; }
    public required Vector2 Anchor { get; set; }
    public required Vector2 Position { get; set; }
    public required float Rotation { get; set; }
    public required Vector2 Scale { get; set; }
    public required Vector4 Color { get; set; }
    public required string MaterialKey { get; set; }
    public required DesiredUniformMap Uniforms { get; set; }
    public required IRenderableSizer Sizer { get; set; }

    public Rectf Tiling { get; set; } = new Rectf(0.0f, 0.0f, 1.0f, 1.0f);
    public Rectf Tiling2 { get; set; } = Rectf.Zero;
    public int NodeId { get; set; } = -1;  // 节点的唯一 ID，用于稳定追踪节点身份
    public Matrix3x3 LocalTransform { get; set; } = Matrix3x3.Identity;
    public Vector2 LocalSize { get; set; } = Vector2.Zero;
    public Type RenderableType { get; set; } = typeof(QuadRenderable);
    public IReadOnlyDictionary<string, object?> CustomArgs { get; set; } = new Dictionary<string, object?>(0);
}