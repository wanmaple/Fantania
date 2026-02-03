using System.Numerics;

namespace FantaniaLib;

public class RenderInfo
{
    public required string Stage { get; set; }
    public required Matrix3x3 Transform { get; set; }
    public required Vector2 Anchor { get; set; }
    public required Vector2 Size { get; set; }
    public required int Depth { get; set; }
    public required int EntityOrder { get; set; }
    public required int LocalOrder { get; set; }
    public required Vector4 Color { get; set; }
    public required string MaterialKey { get; set; }
    public required DesiredUniformMap Uniforms { get; set; }
}