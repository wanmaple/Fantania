using System.Numerics;
using Fantania.Models;

namespace Fantania;

public interface IRenderable
{
    int RealDepth { get; }
    DrawTemplate Template { get; }
    Avalonia.Vector Size { get; }
    Matrix3x3 Transform { get; set; }
    Vector4 VertexColor { get; set; }
    Vector4 Tiling { get; set; }
    Vector4 CustomData { get; set; }
    Vector4 CustomData2 { get; set; }
}