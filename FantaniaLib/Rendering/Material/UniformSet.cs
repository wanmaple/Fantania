using System.Collections;
using System.Numerics;

namespace FantaniaLib;

public class UniformSet : IEquatable<UniformSet>, IReadonlyUniformSet
{
    public struct TextureInformation : IEquatable<TextureInformation>
    {
        public int TextureSlot;
        public int TextureID;    // 惰性初始化，在BVH生命周期内才会赋值。
        public TextureDefinition TextureDef;

        public bool Equals(TextureInformation other)
        {
            return TextureSlot == other.TextureSlot && TextureDef.Equals(other.TextureDef);
        }

        public override bool Equals(object? obj)
        {
            return obj is TextureInformation && Equals((TextureInformation)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TextureSlot, TextureDef);
        }

        public override string ToString()
        {
            return $"slot: {TextureSlot}, def: {TextureDef}";
        }
    }

    public struct TextureArrayInformation : IEquatable<TextureArrayInformation>
    {
        public int[] TextureSlots;
        public int[] TextureIDs;
        public TextureDefinition[] TextureDefs;

        public bool Equals(TextureArrayInformation other)
        {
            if (TextureSlots.Length != other.TextureSlots.Length || TextureIDs.Length != other.TextureIDs.Length || TextureDefs.Length != other.TextureDefs.Length)
                return false;
            for (int i = 0; i < TextureSlots.Length; i++)
            {
                if (TextureSlots[i] != other.TextureSlots[i])
                    return false;
            }
            for (int i = 0; i < TextureDefs.Length; i++)
            {
                if (!TextureDefs[i].Equals(other.TextureDefs[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is TextureArrayInformation && Equals((TextureArrayInformation)obj);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < TextureSlots.Length; i++)
            {
                hash = (hash * 31) ^ TextureSlots[i].GetHashCode();
            }
            for (int i = 0; i < TextureIDs.Length; i++)
            {
                hash = (hash * 31) ^ TextureIDs[i].GetHashCode();
            }
            for (int i = 0; i < TextureDefs.Length; i++)
            {
                hash = (hash * 31) ^ TextureDefs[i].GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return $"count: {TextureDefs.Length}";
        }
    }

    public IReadOnlyCollection<string> Names => _uniforms.Keys;

    public MaterialUniform this[string name]
    {
        get => _uniforms[name];
        set => _uniforms[name] = value;
    }

    public UniformSet()
    {
    }

    public UniformSet(DesiredUniformMap map)
    {
        int texSlot = 0;
        var slotCache = new Dictionary<TextureDefinition, int>(16);
        foreach (var pair in map)
        {
            string name = pair.Key;
            DesiredUniformValue def = pair.Value;
            switch (def.Type)
            {
                case UniformTypes.Int1:
                    SetUniform(name, (int)def.Value);
                    break;
                case UniformTypes.Float1:
                    SetUniform(name, (float)def.Value);
                    break;
                case UniformTypes.Float2:
                    SetUniform(name, (Vector2)def.Value);
                    break;
                case UniformTypes.Float3:
                    SetUniform(name, (Vector3)def.Value);
                    break;
                case UniformTypes.Float4:
                    SetUniform(name, (Vector4)def.Value);
                    break;
                case UniformTypes.Matrix3x3:
                    SetUniform(name, (Matrix3x3)def.Value);
                    break;
                case UniformTypes.Int1Array:
                    SetUniform(name, (int[])def.Value);
                    break;
                case UniformTypes.Float1Array:
                    SetUniform(name, (float[])def.Value);
                    break;
                case UniformTypes.Float2Array:
                    SetUniform(name, (Vector2[])def.Value);
                    break;
                case UniformTypes.Float3Array:
                    SetUniform(name, (Vector3[])def.Value);
                    break;
                case UniformTypes.Float4Array:
                    SetUniform(name, (Vector4[])def.Value);
                    break;
                case UniformTypes.Matrix3x3Array:
                    SetUniform(name, (Matrix3x3[])def.Value);
                    break;
                case UniformTypes.Texture:
                {
                    var texDef = (TextureDefinition)def.Value;
                    if (!slotCache.TryGetValue(texDef, out int singleSlot))
                    {
                        singleSlot = texSlot;
                        slotCache.Add(texDef, singleSlot);
                        texSlot++;
                    }
                    SetUniform(name, texDef, singleSlot);
                    break;
                }
                case UniformTypes.TextureArray:
                {
                    var texDefs = (TextureDefinition[])def.Value;
                    int[] slots = new int[texDefs.Length];
                    for (int i = 0; i < texDefs.Length; i++)
                    {
                        TextureDefinition cur = texDefs[i];
                        if (!slotCache.TryGetValue(cur, out int arrSlot))
                        {
                            arrSlot = texSlot;
                            slotCache.Add(cur, arrSlot);
                            texSlot++;
                        }
                        slots[i] = arrSlot;
                    }
                    SetUniform(name, texDefs, slots);
                    break;
                }
            }
        }
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

    public void SetUniform(string name, int value)
    {
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Int1, value);
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

    public void SetUniform(string name, int[] values)
    {
        int[] copy = (int[])values.Clone();
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Int1Array, copy);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(copy);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, float[] values)
    {
        float[] copy = (float[])values.Clone();
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float1Array, copy);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(copy);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Vector2[] values)
    {
        Vector2[] copy = (Vector2[])values.Clone();
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float2Array, copy);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(copy);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Vector3[] values)
    {
        Vector3[] copy = (Vector3[])values.Clone();
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float3Array, copy);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(copy);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Vector4[] values)
    {
        Vector4[] copy = (Vector4[])values.Clone();
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Float4Array, copy);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(copy);
            _uniforms[name] = val;
        }
    }

    public void SetUniform(string name, Matrix3x3[] values)
    {
        Matrix3x3[] copy = (Matrix3x3[])values.Clone();
        if (!_uniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(UniformTypes.Matrix3x3Array, copy);
            _uniforms.Add(name, val);
        }
        else
        {
            val.Set(copy);
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

    public void SetUniform(string name, TextureDefinition[] values, int[] slots)
    {
        TextureDefinition[] defs = values;
        int[] slotCopy = (int[])slots.Clone();
        int[] ids = new int[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            ids[i] = defs[i].TextureType == TextureTypes.Gpu ? defs[i].TextureParameters.GpuParams.TextureID : 0;
        }
        var info = new TextureArrayInformation
        {
            TextureSlots = slotCopy,
            TextureIDs = ids,
            TextureDefs = defs,
        };
        var uniform = new MaterialUniform(UniformTypes.TextureArray, info);
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