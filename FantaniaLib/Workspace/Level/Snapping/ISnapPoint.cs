using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public enum SnapPointShapes
{
    Circle,
}

public interface ISnapPoint
{
    Vector2 Position { get; }
    SnapPointShapes Shape { get; }
    Color Color { get; }
    float Size { get; }
    bool IsActive { get; set; }
}