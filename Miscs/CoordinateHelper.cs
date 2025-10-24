using Avalonia;

namespace Fantania;

public static class CoordinateHelper
{
    public static Rect CalculateLocalRect(Vector anchor, Vector size)
    {
        double xMin = -anchor.X * size.X;
        double xMax = (1.0 - anchor.X) * size.X;
        double yMin = -anchor.Y * size.Y;
        double yMax = (1.0 - anchor.Y) * size.Y;
        return new Rect(new Point(xMin, yMin), new Point(xMax, yMax));
    }
}