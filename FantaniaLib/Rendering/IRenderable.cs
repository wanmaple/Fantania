using System.Numerics;

namespace FantaniaLib;

public interface IRenderable : IBVHItem
{
    string Stage { get; }
    Matrix3x3 Transform { get; set; }
    Vector2 Anchor { get; }
    Vector2 Size { get; }
    Rectf Tiling { get; }
    Rectf Tiling2 { get; }
    Vector4 VertexColor { get; }
    int Depth { get; set; }
    int EntityOrder { get; set; }
    int LocalOrder { get; set; }
    int NodeIndex { get; set; }
    Mesh Mesh { get; }
    RenderMaterial Material { get; }
}