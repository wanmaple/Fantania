using System.Numerics;
using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential)]
public struct Rectf : IEquatable<Rectf>
{
    public static readonly Rectf Zero = new Rectf();

    public float X;
    public float Y;
    public float Width;
    public float Height;

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;

    public float CenterX => X + Width * 0.5f;
    public float CenterY => Y + Height * 0.5f;

    public Vector2 Position => new Vector2(X, Y);
    public Vector2 Size => new Vector2(Width, Height);
    public Vector2 Center => new Vector2(CenterX, CenterY);
    public Vector2 TopLeft => new Vector2(Left, Top);
    public Vector2 TopRight => new Vector2(Right, Top);
    public Vector2 BottomLeft => new Vector2(Left, Bottom);
    public Vector2 BottomRight => new Vector2(Right, Bottom);

    public float Area => Width * Height;

    public Rectf() : this(0.0f, 0.0f, 0.0f, 0.0f)
    { }

    public Rectf(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Rectf(Vector2 position, Vector2 size)
    : this(position.X, position.Y, size.X, size.Y)
    {
    }

    public static Rectf Merge(Rectf a, Rectf b)
    {
        float left = Math.Min(a.Left, b.Left);
        float top = Math.Min(a.Top, b.Top);
        float right = Math.Max(a.Right, b.Right);
        float bottom = Math.Max(a.Bottom, b.Bottom);
        return new Rectf(left, top, right - left, bottom - top);
    }

    public Rectf Merge(Rectf other) => Merge(this, other);

    public bool Intersects(Rectf other)
    {
        return Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
    }

    public Rectf Intersection(Rectf other)
    {
        if (!Intersects(other))
            return Zero;
        float left = Math.Max(Left, other.Left);
        float top = Math.Max(Top, other.Top);
        float right = Math.Min(Right, other.Right);
        float bottom = Math.Min(Bottom, other.Bottom);
        return new Rectf(left, top, right - left, bottom - top);
    }

    public bool Contains(float x, float y)
    {
        return x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    public bool Contains(Vector2 point) => Contains(point.X, point.Y);

    public bool Contains(Rectf other)
    {
        return Left <= other.Left && Right >= other.Right && Top <= other.Top && Bottom >= other.Bottom;
    }

    public static bool operator ==(Rectf left, Rectf right) => left.Equals(right);
    public static bool operator !=(Rectf left, Rectf right) => !left.Equals(right);

    public bool Equals(Rectf other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object? obj) => obj is Rectf other && Equals(other);

    public override int GetHashCode()
    {
        int hash = (X.GetHashCode() * 397) ^ Y.GetHashCode();
        hash = (hash * 397) ^ Width.GetHashCode();
        hash = (hash * 397) ^ Height.GetHashCode();
        return hash;
    }
}