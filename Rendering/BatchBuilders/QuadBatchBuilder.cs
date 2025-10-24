using System.Collections.Generic;
using System.Numerics;

namespace Fantania;

public class QuadBatchBuilder : IBatchBuilder
{
    public virtual int VertexCount => 4;

    public virtual int IndiceCount => 6;

    public virtual void OnBatching(IRenderable renderable, PositionUVColor[] verts, ushort[] indices, int vertStart, int indiceStart, IReadOnlyDictionary<string, object> additionalData)
    {
        float sizeX = (float)renderable.Size.X;
        float sizeY = (float)renderable.Size.Y;
        // OpenGL coordinates y-axis is upward.
        ushort start = (ushort)vertStart;
        verts[vertStart++] = new PositionUVColor
        {
            Position = new Vector3(renderable.Transform * new Vector2(0.0f, 0.0f), renderable.RealDepth),
            UV = Vector2.UnitY,
            Color = renderable.VertexColor,
            Tiling = renderable.Tiling,
            Custom = renderable.CustomData,
            Custom2 = renderable.CustomData2,
        };
        verts[vertStart++] = new PositionUVColor
        {
            Position = new Vector3(renderable.Transform * new Vector2(sizeX, 0.0f), renderable.RealDepth),
            UV = Vector2.One,
            Color = renderable.VertexColor,
            Tiling = renderable.Tiling,
            Custom = renderable.CustomData,
            Custom2 = renderable.CustomData2,
        };
        verts[vertStart++] = new PositionUVColor
        {
            Position = new Vector3(renderable.Transform * new Vector2(sizeX, sizeY), renderable.RealDepth),
            UV = Vector2.UnitX,
            Color = renderable.VertexColor,
            Tiling = renderable.Tiling,
            Custom = renderable.CustomData,
            Custom2 = renderable.CustomData2,
        };
        verts[vertStart++] = new PositionUVColor
        {
            Position = new Vector3(renderable.Transform * new Vector2(0.0f, sizeY), renderable.RealDepth),
            UV = Vector2.Zero,
            Color = renderable.VertexColor,
            Tiling = renderable.Tiling,
            Custom = renderable.CustomData,
            Custom2 = renderable.CustomData2,
        };
        indices[indiceStart++] = start;
        indices[indiceStart++] = (ushort)(start + 1);
        indices[indiceStart++] = (ushort)(start + 2);
        indices[indiceStart++] = start;
        indices[indiceStart++] = (ushort)(start + 2);
        indices[indiceStart++] = (ushort)(start + 3);
    }
}