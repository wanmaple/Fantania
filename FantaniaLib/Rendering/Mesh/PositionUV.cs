using System.Numerics;
using System.Runtime.InteropServices;

namespace FantaniaLib;

[StructLayout(LayoutKind.Sequential)]
public struct PositionUV
{
    public Vector2 Position;
    public Vector2 UV;

    public override string ToString()
    {
        return $"{Position},{UV}";
    }
}