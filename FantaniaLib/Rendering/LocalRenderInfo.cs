using System.Numerics;

namespace FantaniaLib;

[BindingScript]
public enum ColorOperators
{
    Independent,
    Multiple,
}

public class LocalRenderInfo
{
    public required string Stage { get; set; }
    public required Vector2 Anchor { get; set; }
    public required Vector2 Position { get; set; }
    public required float Rotation { get; set; }
    public required Vector2 Scale { get; set; }
    public required Vector4 Color { get; set; }
    public required ColorOperators ColorOperator { get; set; }
    public required string MaterialKey { get; set; }
    public required DesiredUniformMap Uniforms { get; set; }
    public required IRenderableSizer Sizer { get; set; } 

    public Matrix3x3 LocalTransform { get; set; } = Matrix3x3.Identity;
    public Vector2 LocalSize { get; set; } = Vector2.Zero;
}