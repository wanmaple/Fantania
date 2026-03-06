using System.Numerics;
using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential)]
public struct VertexStandard
{
    public Vector3 Position;
    public Vector4 Color;
    public Vector4 RotationScale; // x = rotation (radians), yz = scale, w is not used.
    public Vector2 UV;
    public Vector2 UV2;

    public override string ToString()
    {
        return $"{Position},{RotationScale},{Color},{UV},{UV2}";
    }
}