using Avalonia;

namespace Fantania;

public interface IBVHItem
{
    Rect BoundingBox { get; }
}