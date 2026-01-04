using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vector2Int : IEquatable<Vector2Int>
{
    public static readonly Vector2Int ZERO = new Vector2Int();
    public static readonly Vector2Int ONE = new Vector2Int(1, 1);

    public Vector2Int() : this(0, 0)
    {}

    public Vector2Int(int x_, int y_)
    {
        x = x_;
        y = y_;
    }

    public bool Equals(Vector2Int other)
    {
        return this == other;
    }

    public override bool Equals(object obj)
    {
        return obj is Vector2Int && Equals((Vector2Int)obj);
    }

    public override int GetHashCode()
    {
        return (x.GetHashCode() * 397) ^ y.GetHashCode();
    }

    public static Vector2Int operator-(Vector2Int v)
    {
        return new Vector2Int(-v.x, -v.y);
    }

    public static Vector2Int operator+(Vector2Int v1, Vector2Int v2)
    {
        return new Vector2Int(v1.x + v2.x, v1.y + v2.y);
    }

    public static Vector2Int operator-(Vector2Int v1, Vector2Int v2)
    {
        return new Vector2Int(v1.x - v2.x, v1.y - v2.y);
    }

    public static Vector2Int operator*(Vector2Int v1, Vector2Int v2)
    {
        return new Vector2Int(v1.x * v2.x, v1.y * v2.y);
    }

    public static Vector2Int operator*(Vector2Int v, int mul)
    {
        return new Vector2Int(v.x * mul, v.y * mul);
    }

    public static bool operator ==(Vector2Int v1, Vector2Int v2)
    {
        return v1.x == v2.x && v1.y == v2.y;
    }

    public static bool operator !=(Vector2Int v1, Vector2Int v2)
    {
        return !(v1 == v2);
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }

    public int x;
    public int y;
}