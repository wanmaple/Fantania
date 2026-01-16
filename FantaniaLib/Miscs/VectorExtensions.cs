using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public static class VectorExtensions
{
    public static float Cross(this Vector2 self, Vector2 other)
    {
        return self.X * other.Y - self.Y * other.X;
    }

    public static Vector2 FlipX(this Vector2 self)
    {
        return new Vector2(-self.X, self.Y);
    }

    public static Vector2 FlipY(this Vector2 self)
    {
        return new Vector2(self.X, -self.Y);
    }

    public static Vector2Int ToGridSpace(this Vector2 self, int gridSize)
    {
        float xInGrid = self.X / gridSize;
        float yInGrid = self.Y / gridSize;
        return new Vector2Int(MathHelper.RoundToInt(xInGrid) * gridSize, MathHelper.RoundToInt(yInGrid) * gridSize);
    }

    public static Vector2 ToVector2(this Vector3 self)
    {
        return new Vector2(self.X, self.Y);
    }

    public static Vector2 ToVector2(this Vector2Int self)
    {
        return new Vector2(self.X, self.Y);
    }

    public static Color ToColor(this Vector4 self)
    {
        return new Color((byte)(self.W * 255), (byte)(self.X * 255), (byte)(self.Y * 255), (byte)(self.Z * 255));
    }

    public static Vector4 ToVector4(this Color self)
    {
        return new Vector4(self.R / 255.0f, self.G / 255.0f, self.B / 255.0f, self.A / 255.0f);
    }

    public static string ToHex(this Color self)
    {
        return $"#{self.A:X2}{self.R:X2}{self.G:X2}{self.B:X2}";
    }
}