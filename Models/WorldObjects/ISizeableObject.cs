using Avalonia;

namespace Fantania.Models;

public interface ISizeableObject
{
    Vector CustomSize { get; set; }
    Vector Anchor { get; set; }
    Vector Position { get; set; }
    double Top { get; }
    double Bottom { get; }
    double Left { get; }
    double Right { get; }
}