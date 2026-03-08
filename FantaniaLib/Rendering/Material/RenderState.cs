using System.Diagnostics.CodeAnalysis;

namespace FantaniaLib;

[BindingScript]
public enum BlendFactors
{
    Zero,
    One,
    SrcAlpha,
    OneMinusSrcAlpha,
    SrcColor,
    OneMinusSrcColor,
    DstAlpha,
    OneMinusDstAlpha,
    DstColor,
    OneMinusDstColor,
}

public struct RenderState : IEquatable<RenderState>
{
    public bool DepthTestEnabled;
    public bool DepthWriteEnabled;
    public bool BlendingEnabled;
    public BlendFactors BlendSrcFactor;
    public BlendFactors BlendDstFactor;
    // TODO: stencil, culling etc...

    public bool Equals(RenderState other)
    {
        return DepthTestEnabled == other.DepthTestEnabled && DepthWriteEnabled == other.DepthWriteEnabled && BlendingEnabled == other.BlendingEnabled && BlendSrcFactor == other.BlendSrcFactor && BlendDstFactor == other.BlendDstFactor;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is RenderState && Equals((RenderState)obj);
    }

    public override int GetHashCode()
    {
        int hash = (DepthTestEnabled.GetHashCode() * 397) ^ DepthWriteEnabled.GetHashCode();
        hash = (hash * 397) ^ BlendingEnabled.GetHashCode();
        hash = (hash * 397) ^ BlendSrcFactor.GetHashCode();
        hash = (hash * 397) ^ BlendDstFactor.GetHashCode();
        return hash;
    }
}