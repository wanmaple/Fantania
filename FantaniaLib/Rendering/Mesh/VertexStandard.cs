using System.Numerics;
using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential)]
public struct VertexStandard
{
    public Vector3 Position;
    public Vector4 Color;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector2 UV;
    public Vector2 UV2;

    public override string ToString()
    {
        return $"{Position},{Color},{Normal},{Tangent},{UV},{UV2}";
    }
}