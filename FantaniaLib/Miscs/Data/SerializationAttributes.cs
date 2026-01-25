namespace FantaniaLib;

[AttributeUsage(AttributeTargets.Property)]
public class SerializableFieldAttribute : Attribute
{
    public FieldTypes FieldType { get; set; }

    public SerializableFieldAttribute(FieldTypes type)
    {
        FieldType = type;
    }
}