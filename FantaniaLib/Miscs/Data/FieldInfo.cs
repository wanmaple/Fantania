namespace FantaniaLib;

[BindingScript]
public enum FieldTypes
{
    Boolean = 0,
    Integer,
    Float,
    String,
    Vector2,
    Vector2Int,
    Color,
    Texture,
    Curve,
    GroupReference,
    TypeReference,
    Enum,

    Custom = 2000,
}

public class FieldInfo
{
    public required string FieldName { get; set; }
    public required FieldTypes FieldType { get; set; }
}