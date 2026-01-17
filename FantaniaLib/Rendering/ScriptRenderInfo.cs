using System.Numerics;

namespace FantaniaLib;

public class ScriptRenderInfo
{
    public required string Stage { get; set; }
    public required int Depth { get; set; }
    public required Vector4 Color { get; set; }
    public required string MaterialKey { get; set; }
    public required DesiredUniformMap Uniforms { get; set; }
    public required IRenderableSizer Sizer { get; set; } 
}