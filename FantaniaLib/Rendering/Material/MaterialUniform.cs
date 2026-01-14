namespace FantaniaLib;

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