using System.Collections;
using System.Numerics;

namespace FantaniaLib;

public class UniformSet : IEquatable<UniformSet>, IEnumerable<KeyValuePair<string, MaterialUniform>>
{
    public struct TextureInformation
    {
        public int TextureSlot;
        public int TextureID;    // 惰性初始化，在BVH生命周期内才会赋值。
        public TextureDefinition TextureDef;
    }

    public IReadOnlyCollection<string> Names => _uniforms.Keys;

    public MaterialUniform this[string name]
    {
        get => _uniforms[name];
        set => _uniforms[name] = value;
    }

    public void SetUniform(string name, MaterialUniform uniform)
    {
        _uniforms[name] = uniform;
    }

    public void SetUniform(string name, float value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float1, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Vector2 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float2, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Vector3 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float3, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Vector4 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float4, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Matrix3x3 value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Matrix3x3, value);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, TextureDefinition value, int slot)
    {
        var info = new TextureInformation
        {
            TextureSlot = slot,
            TextureID = value.TextureType == TextureTypes.Gpu ? value.TextureParameters.GpuParams.TextureID : 0,
            TextureDef = value,
        };
        var uniform = new MaterialUniform(UniformTypes.Texture, info);
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = uniform;
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(info);
            _uniforms[name] = val;
        }
    }

    public UniformSet Clone()
    {
        var clone = new UniformSet();
        foreach (var pair in _uniforms)
        {
            clone.SetUniform(pair.Key, pair.Value);
        }
        return clone;
    }

    internal void Clear()
    {
        _uniforms.Clear();
    }

    public bool Equals(UniformSet? other)
    {
        return other != null && UniformEquals(other._uniforms);
    }

    public override bool Equals(object? obj)
    {
        return obj is UniformSet && Equals((UniformSet)obj);
    }

    public override int GetHashCode()
    {
        int hash = 0;
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

    public IEnumerator<KeyValuePair<string, MaterialUniform>> GetEnumerator()
    {
        return _uniforms.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    Dictionary<string, MaterialUniform> _uniforms = new Dictionary<string, MaterialUniform>(16);
}