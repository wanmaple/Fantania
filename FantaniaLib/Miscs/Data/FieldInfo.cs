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
    Vector3,
    Color,
    Direction3D,
    Texture,
    Curve,
    GroupReference,
    TypeReference,
    Enum,
    Custom = 1000,
    BooleanArray = Boolean + 2000,
    IntegerArray = Integer + 2000,
    FloatArray = Float + 2000,
    StringArray = String + 2000,
    Vector2Array = Vector2 + 2000,
    Vector2IntArray = Vector2Int + 2000,
    Vector3Array = Vector3 + 2000,
    ColorArray = Color + 2000,
    Direction3DArray = Direction3D + 2000,
    TextureArray = Texture + 2000,
    CurveArray = Curve + 2000,
    GroupReferenceArray = GroupReference + 2000,
    TypeReferenceArray = TypeReference + 2000,
    EnumArray = Enum + 2000,
    CustomArray = Custom + 2000,
}

public class FieldInfo
{
    public required string FieldName { get; set; }
    public required FieldTypes FieldType { get; set; }
}