using System.Collections.Generic;

namespace Fantania;

public interface IBatchBuilder
{
    int VertexCount { get; }
    int IndiceCount { get; }

    void OnBatching(IRenderable renderable, PositionUVColor[] verts, ushort[] indices, int vertStart, int indiceStart, IReadOnlyDictionary<string, object> additionalData);
}