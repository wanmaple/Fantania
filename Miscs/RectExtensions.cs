using System;
using Avalonia;

namespace Fantania;

public static class RectExtensions
{
    public static bool IsZero(this Rect rect)
    {
        return rect.Width == 0 && rect.Height == 0;
    }

    public static double Area(this Rect rect)
    {
        return rect.Width * rect.Height;
    }

    public static Rect Merge(this Rect lhs, Rect rhs)
    {
        double minX = Math.Min(lhs.Position.X, rhs.Position.X);
        double minY = Math.Min(lhs.Position.Y, rhs.Position.Y);
        double maxX = Math.Max(lhs.Position.X + lhs.Size.Width, rhs.Position.X + rhs.Size.Width);
        double maxY = Math.Max(lhs.Position.Y + lhs.Size.Height, rhs.Position.Y + rhs.Size.Height);
        return new Rect(new Point(minX, minY), new Point(maxX, maxY));
    }
}