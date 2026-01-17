namespace FantaniaLib;

public struct TypeReference : IEquatable<TypeReference>
{
    public static readonly TypeReference None = new TypeReference
    {
        ReferenceType = string.Empty,
        ReferenceID = 0,
    };

    public string ReferenceType;
    public int ReferenceID;

    public TypeReference(string type, int id)
    {
        ReferenceType = type;
        ReferenceID = id;
    }

    public static bool operator ==(TypeReference lhs, TypeReference rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(TypeReference lhs, TypeReference rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(TypeReference other)
    {
        return ReferenceType == other.ReferenceType && ReferenceID == other.ReferenceID;
    }

    public override bool Equals(object? obj)
    {
        return obj is TypeReference && Equals((TypeReference)obj);
    }

    public override int GetHashCode()
    {
        return (ReferenceType.GetHashCode() * 397) ^ ReferenceID.GetHashCode();
    }
}