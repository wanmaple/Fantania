namespace FantaniaLib;

public class RenderMaterial : IEquatable<RenderMaterial>
{
    public required ShaderProgram Shader { get; set; }
    public UniformSet Uniforms => _uniforms;

    public RenderMaterial Clone()
    {
        var clone = new RenderMaterial
        {
            Shader = Shader,
            _uniforms = _uniforms.Clone(),
        };
        return clone;
    }

    public bool Equals(RenderMaterial? other)
    {
        return other != null && Shader.Equals(other.Shader) && _uniforms.Equals(other._uniforms);
    }

    public override bool Equals(object? obj)
    {
        return obj is RenderMaterial && Equals((RenderMaterial)obj);
    }

    public override int GetHashCode()
    {
        int hash = (Shader.GetHashCode() * 397) ^ _uniforms.GetHashCode();
        return hash;
    }

    UniformSet _uniforms = new UniformSet();
}