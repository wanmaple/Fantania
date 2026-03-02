using System.Collections;
using System.Numerics;

namespace FantaniaLib;

public struct DesiredUniformValue
{
    public UniformTypes Type;
    public object Value;       // Texture类型存TextureDefinition，TextureArray类型存TextureDefinition[]。
}

public class DesiredUniformMap : IEnumerable<KeyValuePair<string, DesiredUniformValue>>
{
    public DesiredUniformMap()
    {
    }

    public void SetUniform(string key, DesiredUniformValue value)
    {
        _uniforms[key] = value;
    }

    public void ApplyToUniformSet(UniformSet set)
    {
        set.Clear();
        int texSlot = 0;
        var slotCache = new Dictionary<TextureDefinition, int>(16);
        foreach (var pair in _uniforms)
        {
            string name = pair.Key;
            DesiredUniformValue def = pair.Value;
            switch (def.Type)
            {
                case UniformTypes.Int1:
                    set.SetUniform(name, (int)def.Value);
                    break;
                case UniformTypes.Float1:
                    set.SetUniform(name, (float)def.Value);
                    break;
                case UniformTypes.Float2:
                    set.SetUniform(name, (Vector2)def.Value);
                    break;
                case UniformTypes.Float3:
                    set.SetUniform(name, (Vector3)def.Value);
                    break;
                case UniformTypes.Float4:
                    set.SetUniform(name, (Vector4)def.Value);
                    break;
                case UniformTypes.Matrix3x3:
                    set.SetUniform(name, (Matrix3x3)def.Value);
                    break;
                case UniformTypes.Int1Array:
                    set.SetUniform(name, (int[])def.Value);
                    break;
                case UniformTypes.Float1Array:
                    set.SetUniform(name, (float[])def.Value);
                    break;
                case UniformTypes.Float2Array:
                    set.SetUniform(name, (Vector2[])def.Value);
                    break;
                case UniformTypes.Float3Array:
                    set.SetUniform(name, (Vector3[])def.Value);
                    break;
                case UniformTypes.Float4Array:
                    set.SetUniform(name, (Vector4[])def.Value);
                    break;
                case UniformTypes.Matrix3x3Array:
                    set.SetUniform(name, (Matrix3x3[])def.Value);
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
                    set.SetUniform(name, texDef, singleSlot);
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
                    set.SetUniform(name, texDefs, slots);
                    break;
                }
            }
        }
    }

    public IEnumerator<KeyValuePair<string, DesiredUniformValue>> GetEnumerator()
    {
        return _uniforms.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    Dictionary<string, DesiredUniformValue> _uniforms = new Dictionary<string, DesiredUniformValue>(0);
}