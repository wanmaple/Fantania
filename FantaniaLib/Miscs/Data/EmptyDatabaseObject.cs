namespace FantaniaLib;

public class EmptyDatabaseObject : DatabaseObject
{
    public static readonly EmptyDatabaseObject Instance = new EmptyDatabaseObject();

    public override string TypeName => GetType().Name;
    public override string GroupName => string.Empty;

    public override string Name => "None";

    private EmptyDatabaseObject() : base(0)
    {
    }
}