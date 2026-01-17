using System.Collections;

namespace FantaniaLib;

public struct DesiredUniformValue
{
    public UniformTypes Type;
    public object Value;       // 如果Type为UniformTypes.Texture，这个时候由于缺失上下文，Value存储的应该是TextureDefinition。
}

/// <summary>
/// 用于记录uniform的字面量，这个期间一般没有渲染上下文，所以与渲染有关的信息无法得知。
/// </summary>
public class DesiredUniformMap : IEnumerable<KeyValuePair<string, DesiredUniformValue>>
{
    public DesiredUniformMap()
    {
    }

    public void SetUniform(string key, DesiredUniformValue value)
    {
        _uniforms[key] = value;
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