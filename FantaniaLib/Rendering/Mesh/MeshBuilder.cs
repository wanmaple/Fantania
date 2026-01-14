using System.Numerics;

namespace FantaniaLib;

public static class MeshBuilder
{
    public static Mesh CreateScreenQuad()
    {
        PositionUV[] quadVerts = [
            new PositionUV { Position = Vector2.Zero, UV = Vector2.Zero, },
            new PositionUV { Position = Vector2.UnitX, UV = Vector2.UnitX, },
            new PositionUV { Position = Vector2.One, UV = Vector2.One, },
            new PositionUV { Position = Vector2.UnitY, UV = Vector2.UnitY, },
        ];
        ushort[] quadIndices = [0, 1, 2, 0, 2, 3];
        return Mesh.From(quadVerts, quadIndices);
    }
}