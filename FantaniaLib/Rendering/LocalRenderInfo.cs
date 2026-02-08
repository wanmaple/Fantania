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

    public int NodeId { get; set; }  // 节点的唯一 ID，用于稳定追踪节点身份
    public Matrix3x3 LocalTransform { get; set; } = Matrix3x3.Identity;
    public Vector2 LocalSize { get; set; } = Vector2.Zero;
}