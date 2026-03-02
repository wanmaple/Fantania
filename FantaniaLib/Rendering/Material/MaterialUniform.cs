namespace FantaniaLib;

[BindingScript]
public enum UniformTypes
{
    Int1,
    Float1,
    Float2,
    Float3,
    Float4,
    Matrix3x3,
    Int1Array,
    Float1Array,
    Float2Array,
    Float3Array,
    Float4Array,
    Matrix3x3Array,
    Texture,
    TextureArray,
}

public struct MaterialUniform : IEquatable<MaterialUniform>
{
    public UniformTypes Type => _type;

    public MaterialUniform(UniformTypes type, object value)
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

    public bool Equals(MaterialUniform other)
    {
        return _type == other._type && ValueEquals(_value, other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is MaterialUniform && Equals((MaterialUniform)obj);
    }

    public override int GetHashCode()
    {
        return (_type.GetHashCode() * 397) ^ ValueHash(_value);
    }

    static bool ValueEquals(object lhs, object rhs)
    {
        if (ReferenceEquals(lhs, rhs))
            return true;
        if (lhs is Array arrL && rhs is Array arrR)
        {
            if (arrL.Length != arrR.Length)
                return false;
            for (int i = 0; i < arrL.Length; i++)
            {
                object? l = arrL.GetValue(i);
                object? r = arrR.GetValue(i);
                if (!Equals(l, r))
                    return false;
            }
            return true;
        }
        return lhs.Equals(rhs);
    }

    static int ValueHash(object value)
    {
        if (value is Array arr)
        {
            int hash = 17;
            for (int i = 0; i < arr.Length; i++)
            {
                hash = (hash * 31) ^ (arr.GetValue(i)?.GetHashCode() ?? 0);
            }
            return hash;
        }
        return value.GetHashCode();
    }

    UniformTypes _type;
    object _value;
}