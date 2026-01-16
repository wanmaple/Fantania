using System.Numerics;

namespace FantaniaLib;

public class RenderInfo
{
    public required string Stage { get; set; }
    public required Vector2 Anchor { get; set; }
    public required Vector2 Size { get; set; }
    public required Vector2 Position { get; set; }
    public required int Depth { get; set; }
    public required float Rotation { get; set; }
    public required Vector2 Scale { get; set; }
    public required Vector4 Color { get; set; }
    public required string MaterialKey { get; set; }
    public required IReadOnlyDictionary<string, object> Uniforms { get; set; }
}