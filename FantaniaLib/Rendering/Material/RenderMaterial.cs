using System.Numerics;

namespace FantaniaLib;

public class RenderMaterial : IEquatable<RenderMaterial>
{
    public required ShaderProgram Shader { get; set; }
    public IReadOnlyDictionary<string, MaterialUniform> Uniforms => _uniforms;

    public void SetUniformVar(string name, object value)
    {
        if (value is float f)
            SetUniform(name, f);
        else if (value is Vector2 v2)
            SetUniform(name, v2);
        else if (value is Vector3 v3)
            SetUniform(name, v3);
        else if (value is Vector4 v4)
            SetUniform(name, v4);
        else if (value is Matrix3x3 mat)
            SetUniform(name, mat);
        else if (value is (int slot, int texId))
            SetUniform(name, (slot, texId));
    }

    public void SetUniform(string name, MaterialUniform uniform)
    {
        _uniforms[name] = uniform;
    }

    public void SetUniform(string name, float value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float1, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name ] = val;
        }
    }

    public void SetUniform(string name, Vector2 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float2, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name ] = val;
        }
    }

    public void SetUniform(string name, Vector3 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float3, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name ] = val;
        }
    }

    public void SetUniform(string name, Vector4 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float4, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name ] = val;
        }
    }

    public void SetUniform(string name, Matrix3x3 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Matrix3x3, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name ] = val;
        }
    }

    public void SetUniform(string name, (int slot, int texId) value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Texture, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name ] = val;
        }
    }

    public RenderMaterial Clone()
    {
        var clone = new RenderMaterial
        {
            Shader = Shader,
            _uniforms = new Dictionary<string, MaterialUniform>(_uniforms.Count),
        };
        foreach (var pair in _uniforms)
        {
            clone._uniforms.Add(pair.Key, pair.Value.Clone());
        }
        return clone;
    }

    public bool Equals(RenderMaterial? other)
    {
        return other != null && Shader.Equals(other.Shader) && UniformEquals(other._uniforms);
    }

    public override bool Equals(object? obj)
    {
        return obj is RenderMaterial && Equals((RenderMaterial)obj);
    }

    public override int GetHashCode()
    {
        int hash = Shader.GetHashCode();
        foreach (var pair in _uniforms)
        {
            hash = (hash * 397) ^ pair.Key.GetHashCode();
            hash = (hash * 397) ^ pair.Value.GetHashCode();
        }
        return hash;
    }

    bool UniformEquals(IReadOnlyDictionary<string, MaterialUniform> other)
    {
        if (_uniforms.Count != other.Count) return false;
        foreach (var pair in _uniforms)
        {
            if (!other.TryGetValue(pair.Key, out var otherUniform))
                return false;
            if (!pair.Value.Equals(otherUniform))
                return false;
        }
        return true;
    }

    Dictionary<string, MaterialUniform> _uniforms = new Dictionary<string, MaterialUniform>(16);
}