using System;
using System.Numerics;

namespace Fantania;

public enum GradientShapes
{
    Linear,
    Radial,
}

public struct Gradient2D : IEquatable<Gradient2D>
{
    public static readonly Gradient2D Default = new Gradient2D();

    public GradientShapes Shape { get; set; }
    public Vector2 BeginAnchor { get; set; }
    public Vector2 EndAnchor { get; set; }
    public Gradient1D Gradient { get; set; }

    public Gradient2D()
    : this(Gradient1D.BlackWhite, new Vector2(0.5f, 0.0f), new Vector2(0.5f, 1.0f))
    {
    }

    public Gradient2D(Gradient1D gradient, Vector2 beginAnchor, Vector2 endAnchor)
    {
        Shape = GradientShapes.Linear;
        Gradient = gradient;
        BeginAnchor = beginAnchor;
        EndAnchor = endAnchor;
    }

    public Vector4 Evaluate(Vector2 uv)
    {
        Vector2 vec = EndAnchor - BeginAnchor;
        if (BeginAnchor == EndAnchor)
            return Gradient.Evaluate(0.0f);
        Vector2 toBegin = uv - BeginAnchor;
        switch (Shape)
        {
            case GradientShapes.Radial:
                float t = MathHelper.Clamp(toBegin.Length() / vec.Length(), 0.0f, 1.0f);
                return Gradient.Evaluate(t);
            case GradientShapes.Linear:
            default:
                float dot = Vector2.Dot(toBegin, vec) / vec.LengthSquared();
                dot = MathHelper.Clamp(dot, 0.0f, 1.0f);
                return Gradient.Evaluate(dot);
        }
    }

    public bool Equals(Gradient2D other)
    {
        return this == other;
    }

    public override bool Equals(object obj)
    {
        return obj is Gradient2D && Equals((Gradient2D)obj);
    }

    public override int GetHashCode()
    {
        int ret = BeginAnchor.GetHashCode();
        ret = (ret * 397) ^ EndAnchor.GetHashCode();
        ret = (ret * 397) ^ Gradient.GetHashCode();
        return ret;
    }

    public Gradient2D Clone()
    {
        var ret = new Gradient2D();
        ret.Shape = Shape;
        ret.BeginAnchor = BeginAnchor;
        ret.EndAnchor = EndAnchor;
        ret.Gradient = Gradient.Clone();
        return ret;
    }

    public static bool operator ==(Gradient2D lhs, Gradient2D rhs)
    {
        return lhs.Shape == rhs.Shape && lhs.BeginAnchor == rhs.BeginAnchor && lhs.EndAnchor == rhs.EndAnchor && lhs.Gradient == rhs.Gradient;
    }

    public static bool operator !=(Gradient2D lhs, Gradient2D rhs)
    {
        return !(lhs == rhs);
    }
}