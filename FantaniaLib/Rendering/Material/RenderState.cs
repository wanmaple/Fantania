using System.Diagnostics.CodeAnalysis;

namespace FantaniaLib;

public struct RenderState : IEquatable<RenderState>
{
    public bool DepthTestEnabled;
    public bool DepthWriteEnabled;
    public bool BlendingEnabled;
    public int BlendFuncSrc;
    public int BlendFuncDst;
    // TODO: stencil, culling etc...

    public bool Equals(RenderState other)
    {
        return DepthTestEnabled == other.DepthTestEnabled && DepthWriteEnabled == other.DepthWriteEnabled && BlendingEnabled == other.BlendingEnabled && BlendFuncSrc == other.BlendFuncSrc && BlendFuncDst == other.BlendFuncDst;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is RenderState && Equals((RenderState)obj);
    }

    public override int GetHashCode()
    {
        int hash = (DepthTestEnabled.GetHashCode() * 397) ^ DepthWriteEnabled.GetHashCode();
        hash = (hash * 397) ^ BlendingEnabled.GetHashCode();
        hash = (hash * 397) ^ BlendFuncSrc.GetHashCode();
        hash = (hash * 397) ^ BlendFuncDst.GetHashCode();
        return hash;
    }
}