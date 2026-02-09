using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential)]
public struct Recti : IEquatable<Recti>
{
    public static readonly Recti Zero = new Recti();

    public int X;
    public int Y;
    public int Width;
    public int Height;

    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;

    public Vector2Int Position => new Vector2Int(X, Y);
    public Vector2Int Size => new Vector2Int(Width, Height);
    public Vector2Int TopLeft => new Vector2Int(Left, Top);
    public Vector2Int TopRight => new Vector2Int(Right, Top);
    public Vector2Int BottomLeft => new Vector2Int(Left, Bottom);
    public Vector2Int BottomRight => new Vector2Int(Right, Bottom);

    public int Area => Width * Height;
    public bool IsZero => Width == 0 || Height == 0;

    public Recti() : this(0, 0, 0, 0)
    { }

    public Recti(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Recti(Vector2Int position, Vector2Int size)
    : this(position.X, position.Y, size.X, size.Y)
    {
    }

    public static Recti Merge(Recti a, Recti b)
    {
        if (a.IsZero) return b;
        if (b.IsZero) return a;
        int left = Math.Min(a.Left, b.Left);
        int top = Math.Min(a.Top, b.Top);
        int right = Math.Max(a.Right, b.Right);
        int bottom = Math.Max(a.Bottom, b.Bottom);
        return new Recti(left, top, right - left, bottom - top);
    }

    public Recti Merge(Recti other) => Merge(this, other);

    public bool Intersects(Recti other)
    {
        return Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
    }

    public Recti Intersection(Recti other)
    {
        if (!Intersects(other))
            return Zero;
        int left = Math.Max(Left, other.Left);
        int top = Math.Max(Top, other.Top);
        int right = Math.Min(Right, other.Right);
        int bottom = Math.Min(Bottom, other.Bottom);
        return new Recti(left, top, right - left, bottom - top);
    }

    public bool Contains(int x, int y)
    {
        return x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    public bool Contains(Vector2Int point) => Contains(point.X, point.Y);

    public bool Contains(Recti other)
    {
        return Left <= other.Left && Right >= other.Right && Top <= other.Top && Bottom >= other.Bottom;
    }

    public static bool operator ==(Recti left, Recti right) => left.Equals(right);
    public static bool operator !=(Recti left, Recti right) => !left.Equals(right);

    public bool Equals(Recti other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object? obj) => obj is Recti other && Equals(other);

    public override int GetHashCode()
    {
        int hash = (X.GetHashCode() * 397) ^ Y.GetHashCode();
        hash = (hash * 397) ^ Width.GetHashCode();
        hash = (hash * 397) ^ Height.GetHashCode();
        return hash;
    }
}