using System.Collections;
using System.Numerics;

namespace FantaniaLib;

public struct DesiredUniformValue
{
    public UniformTypes Type;
    public object Value;       // 如果Type为UniformTypes.Texture，Value存储的是TextureDefinition。
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
        foreach (var pair in _uniforms)
        {
            string name = pair.Key;
            DesiredUniformValue def = pair.Value;
            switch (def.Type)
            {
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
                case UniformTypes.Texture:
                    var texDef = (TextureDefinition)def.Value;
                    set.SetUniform(name, texDef, texSlot);
                    texSlot++;
                    break;
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