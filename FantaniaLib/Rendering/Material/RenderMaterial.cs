using System.Numerics;

namespace FantaniaLib;

public class RenderMaterial : IEquatable<RenderMaterial>
{
    public class MaterialUniform : IEquatable<MaterialUniform>
    {
        public enum UniformType
        {
            Float1,
            Float2,
            Float3,
            Float4,
            Matrix3x3,
            Texture,
        }

        public UniformType Type => _type;

        public MaterialUniform(UniformType type, object value)
        {
            _type = type;
            _value = value;
        }

        public T Get<T>()
        {
            return (T)_value;
        }

        public void Set(object value)
        {
            _value = value;
        }

        public MaterialUniform Clone()
        {
            return new MaterialUniform(_type, _value);
        }

        public bool Equals(MaterialUniform? other)
        {
            return other != null && _type == other._type && _value.Equals(other._value);
        }

        public override bool Equals(object? obj)
        {
            return obj is MaterialUniform && Equals((MaterialUniform)obj);
        }

        public override int GetHashCode()
        {
            return (_type.GetHashCode() * 397) ^ _value.GetHashCode();
        }

        UniformType _type;
        object _value;
    }

    public required RenderState State { get; set; }
    public required ShaderProgram Shader { get; set; }
    public IReadOnlyDictionary<string, MaterialUniform> Uniforms => _uniforms;

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
        }
    }

    public void SetTexture(string name, int slot, int texId)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Texture, (slot, texId));
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set((slot, texId));
        }
    }

    public RenderMaterial Clone()
    {
        var clone = new RenderMaterial
        {
            State = State,
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
        return other != null && State.Equals(other.State) && Shader.Equals(other.Shader) && UniformEquals(other._uniforms);
    }

    public override bool Equals(object? obj)
    {
        return obj is RenderMaterial && Equals((RenderMaterial)obj);
    }

    public override int GetHashCode()
    {
        int hash = (State.GetHashCode() * 397) ^ Shader.GetHashCode();
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