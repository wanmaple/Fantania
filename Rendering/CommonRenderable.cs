using System.Numerics;
using Fantania.Models;

namespace Fantania;

public class CommonRenderable : IRenderable
{
    public int RealDepth { get; set; }

    public DrawTemplate Template { get; set; }

    public Avalonia.Vector Size { get; set; }

    public Matrix3x3 Transform { get; set; }
    public Vector4 VertexColor { get; set; }
    public Vector4 Tiling { get; set; }
    public Vector4 CustomData { get; set; }
    public Vector4 CustomData2 { get; set; }
}
