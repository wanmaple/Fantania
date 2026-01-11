namespace FantaniaLib;

[BindingScript]
public enum FieldTypes
{
    Boolean = 0,
    Integer,
    Float,
    String,
    Vector2,
    Color,
    Texture,
    Curve,
    GroupReference,
    TypeReference,
}

public class FieldInfo
{
    public required string FieldName { get; set; }
    public required FieldTypes FieldType { get; set; }
}