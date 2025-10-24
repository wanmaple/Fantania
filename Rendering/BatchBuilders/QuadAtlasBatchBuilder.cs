using System.Collections.Generic;
using Avalonia;

namespace Fantania;

public class QuadAtlasBatchBuilder : QuadBatchBuilder
{
    public override void OnBatching(IRenderable renderable, PositionUVColor[] verts, ushort[] indices, int vertStart, int indiceStart, IReadOnlyDictionary<string, object> additionalData)
    {
        base.OnBatching(renderable, verts, indices, vertStart, indiceStart, additionalData);
        Rect uvRect = (Rect)additionalData["uv_rect"];
        verts[vertStart++].UV = new System.Numerics.Vector2((float)uvRect.Left, (float)uvRect.Bottom);
        verts[vertStart++].UV = new System.Numerics.Vector2((float)uvRect.Right, (float)uvRect.Bottom);
        verts[vertStart++].UV = new System.Numerics.Vector2((float)uvRect.Right, (float)uvRect.Top);
        verts[vertStart++].UV = new System.Numerics.Vector2((float)uvRect.Left, (float)uvRect.Top);
    }
}