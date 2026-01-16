namespace FantaniaLib;

public interface IRenderable : IBVHItem
{
    string Stage { get; }
    Matrix3x3 Transform { get; }
    int Depth { get; }
    Mesh Mesh { get; }
    RenderMaterial Material { get; }
}