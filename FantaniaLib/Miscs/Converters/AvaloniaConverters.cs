using System.Numerics;

namespace FantaniaLib;

public static class AvaloniaConverters
{
    public static Vector2 ToVector2(this Avalonia.Vector self)
    {
        return new Vector2((float)self.X, (float)self.Y);
    }

    public static Vector2 ToVector2(this Avalonia.Point self)
    {
        return new Vector2((float)self.X, (float)self.Y);
    }
}