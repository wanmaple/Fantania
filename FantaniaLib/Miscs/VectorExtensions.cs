using System.Numerics;

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

    public static Vector2 ToVector2(this Vector2Int self)
    {
        return new Vector2(self.X, self.Y);
    }

    public static Vector2Int ToGridSpace(this Vector2 self, int gridSize)
    {
        float xInGrid = self.X / gridSize;
        float yInGrid = self.Y / gridSize;
        return new Vector2Int(MathHelper.RoundToInt(xInGrid) * gridSize, MathHelper.RoundToInt(yInGrid) * gridSize);
    }

    public static Vector2 XY(this Vector3 self)
    {
        return new Vector2(self.X, self.Y);
    }

    public static Vector2 XZ(this Vector3 self)
    {
        return new Vector2(self.X, self.Z);
    }

    public static Vector2 YZ(this Vector3 self)
    {
        return new Vector2(self.Y, self.Z);
    }
}