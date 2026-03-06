namespace FantaniaLib;

public struct Direction3D : IEquatable<Direction3D>
{
    public float Azimuth;   // Azimuth=0时XY方向朝右，顺时针增加
    public float Elevation; // Elevation>0时朝内，Elevation<0时朝外

    public Direction3D()
    : this(0.0f, 0.0f)
    {
    }

    public Direction3D(float azimuth, float elevation)
    {
        Azimuth = azimuth;
        Elevation = elevation;
    }

    public static bool operator ==(Direction3D lhs, Direction3D rhs)
    {
        return lhs.Azimuth == rhs.Azimuth && lhs.Elevation == rhs.Elevation;
    }

    public static bool operator !=(Direction3D lhs, Direction3D rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(Direction3D other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        return obj is Direction3D && Equals((Direction3D)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Azimuth, Elevation);
    }

    public override string ToString()
    {
        return $"({Azimuth:F2}°, {Elevation:F2}°)";
    }
}