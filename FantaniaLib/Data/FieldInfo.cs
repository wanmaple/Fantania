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

    BooleanArray = Boolean + 100,
    IntegerArray,
    FloatArray,
    StringArray,
    Vector2Array,
    ColorArray,
    TextureArray,
    CurveArray,
}

public class FieldInfo
{
    public string FieldName { get; set; }
    public FieldTypes FieldType { get; set; }
}