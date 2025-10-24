using System.Numerics;
using System.Runtime.InteropServices;

namespace Fantania;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct PositionUVColor
{
    public const int LOCATION_POSITION = 0;
    public const int LOCATION_UV = 1;
    public const int LOCATION_COLOR = 2;
    public const int LOCATION_TILING = 3;
    public const int LOCATION_CUSTOM = 4;
    public const int LOCATION_CUSTOM2 = 5;

    public Vector3 Position;
    public Vector2 UV;
    public Vector4 Color;
    public Vector4 Tiling;
    public Vector4 Custom;
    public Vector4 Custom2;

    public override string ToString()
    {
        return $"Pos: {Position}, UV: {UV}, Color: {Color}, Tiling: {Tiling}, Custom: {Custom}, Custom2: {Custom2}";
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FullScreenVertice
{
    public const int LOCATION_POSITION = 0;
    public const int LOCATION_UV = 1;
    
    public Vector2 Position;
    public Vector2 UV;
}