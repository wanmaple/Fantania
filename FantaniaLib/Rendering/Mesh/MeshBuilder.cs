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

    public static Mesh CreateStandardQuad()
    {
        return CreateStandardQuad(new Vector2(1.0f, 1.0f));
    }

    public static Mesh CreateStandardQuad(Vector2 size)
    {
        VertexStandard[] quadVerts = [
            new VertexStandard { Position = new Vector3(0.0f, 0.0f, 0.0f), UV = Vector2.Zero, Normal = Vector3.UnitZ, Color = Vector4.One, Tangent = Vector3.UnitX, },
            new VertexStandard { Position = new Vector3(size.X, 0.0f, 0.0f), UV = Vector2.UnitX, Normal = Vector3.UnitZ, Color = Vector4.One, Tangent = Vector3.UnitX, },
            new VertexStandard { Position = new Vector3(size.X, size.Y, 0.0f), UV = Vector2.One, Normal = Vector3.UnitZ, Color = Vector4.One, Tangent = Vector3.UnitX, },
            new VertexStandard { Position = new Vector3(0.0f, size.Y, 0.0f), UV = Vector2.UnitY, Normal = Vector3.UnitZ, Color = Vector4.One, Tangent = Vector3.UnitX, },
        ];
        ushort[] quadIndices = [0, 1, 2, 0, 2, 3];
        return Mesh.From(quadVerts, quadIndices);
    }
}