using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vector2Int : IEquatable<Vector2Int>
{
    public static readonly Vector2Int Zero = new Vector2Int();
    public static readonly Vector2Int One = new Vector2Int(1, 1);

    public int X;
    public int Y;

    public Vector2Int() : this(0, 0)
    {}

    public Vector2Int(int x_, int y_)
    {
        X = x_;
        Y = y_;
    }

    public bool Equals(Vector2Int other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2Int && Equals((Vector2Int)obj);
    }

    public override int GetHashCode()
    {
        return (X.GetHashCode() * 397) ^ Y.GetHashCode();
    }

    public static Vector2Int operator-(Vector2Int v)
    {
        return new Vector2Int(-v.X, -v.Y);
    }

    public static Vector2Int operator+(Vector2Int v1, Vector2Int v2)
    {
        return new Vector2Int(v1.X + v2.X, v1.Y + v2.Y);
    }

    public static Vector2Int operator-(Vector2Int v1, Vector2Int v2)
    {
        return new Vector2Int(v1.X - v2.X, v1.Y - v2.Y);
    }

    public static Vector2Int operator*(Vector2Int v1, Vector2Int v2)
    {
        return new Vector2Int(v1.X * v2.X, v1.Y * v2.Y);
    }

    public static Vector2Int operator*(Vector2Int v, int mul)
    {
        return new Vector2Int(v.X * mul, v.Y * mul);
    }

    public static bool operator ==(Vector2Int v1, Vector2Int v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y;
    }

    public static bool operator !=(Vector2Int v1, Vector2Int v2)
    {
        return !(v1 == v2);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}