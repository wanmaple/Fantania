namespace FantaniaLib;

public struct GroupReference : IEquatable<GroupReference>
{
    public static readonly GroupReference None = new GroupReference
    {
        ReferenceGroup = string.Empty,
        ReferenceID = 0,
    };

    public string ReferenceGroup;
    public int ReferenceID;

    public static bool operator ==(GroupReference lhs, GroupReference rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(GroupReference lhs, GroupReference rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(GroupReference other)
    {
        return ReferenceGroup == other.ReferenceGroup && ReferenceID == other.ReferenceID;
    }

    public override bool Equals(object? obj)
    {
        return obj is GroupReference && Equals((GroupReference)obj);
    }

    public override int GetHashCode()
    {
        return (ReferenceGroup.GetHashCode() * 397) ^ ReferenceID.GetHashCode();
    }
}