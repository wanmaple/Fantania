using System.Numerics;
using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class ConversionHelper
{
    public static object? FieldTypeToValue(FieldTypes type, DynValue val)
    {
        return type switch
        {
            FieldTypes.Boolean => val.GetBooleanOrDefault(false),
            FieldTypes.Integer => val.GetIntegerOrDefault(0),
            FieldTypes.Float => val.GetFloatOrDefault(0.0f),
            FieldTypes.String => val.GetStringOrDefault(string.Empty),
            FieldTypes.Vector2 => val.GetObjectOrDefault(Vector2.Zero),
            FieldTypes.Vector2Int => val.GetObjectOrDefault(Vector2Int.Zero),
            FieldTypes.Vector3 => val.GetObjectOrDefault(Vector3.Zero),
            FieldTypes.Color => val.GetObjectOrDefault(Vector4.One),
            FieldTypes.Texture => val.GetObjectOrDefault(TextureDefinition.None),
            FieldTypes.GroupReference => val.GetObjectOrDefault(GroupReference.None),
            FieldTypes.TypeReference => val.GetObjectOrDefault(TypeReference.None),
            _ => null,
        };
    }

    public static FieldTypes TypeToFieldType(Type type)
    {
        if (type == typeof(bool)) return FieldTypes.Boolean;
        if (type == typeof(int)) return FieldTypes.Integer;
        if (type == typeof(float)) return FieldTypes.Float;
        if (type == typeof(string)) return FieldTypes.String;
        if (type == typeof(Vector2)) return FieldTypes.Vector2;
        if (type == typeof(Vector2Int)) return FieldTypes.Vector2Int;
        if (type == typeof(Vector3)) return FieldTypes.Vector3;
        if (type == typeof(Vector4)) return FieldTypes.Color;
        if (type == typeof(TextureDefinition)) return FieldTypes.Texture;
        if (type == typeof(GroupReference)) return FieldTypes.GroupReference;
        if (type == typeof(TypeReference)) return FieldTypes.TypeReference;
        throw new ArgumentException($"Unsupported type: {type}");
    }

    public static object UniformTypeToValue(UniformTypes type, DynValue val)
    {
        return type switch
        {
            UniformTypes.Int1 => val.GetIntegerOrDefault(0),
            UniformTypes.Float1 => val.GetFloatOrDefault(0.0f),
            UniformTypes.Float2 => val.GetObjectOrDefault(Vector2.Zero),
            UniformTypes.Float3 => val.GetObjectOrDefault(Vector3.Zero),
            UniformTypes.Float4 => val.GetObjectOrDefault(Vector4.Zero),
            UniformTypes.Matrix3x3 => val.GetObjectOrDefault(Matrix3x3.Identity),
            UniformTypes.Int1Array => val.GetObjectOrDefault(Array.Empty<int>()),
            UniformTypes.Float1Array => val.GetObjectOrDefault(Array.Empty<float>()),
            UniformTypes.Float2Array => val.GetObjectOrDefault(Array.Empty<Vector2>()),
            UniformTypes.Float3Array => val.GetObjectOrDefault(Array.Empty<Vector3>()),
            UniformTypes.Float4Array => val.GetObjectOrDefault(Array.Empty<Vector4>()),
            UniformTypes.Matrix3x3Array => val.GetObjectOrDefault(Array.Empty<Matrix3x3>()),
            UniformTypes.Texture => val.GetObjectOrDefault(TextureDefinition.None),
            UniformTypes.TextureArray => val.GetObjectOrDefault(Array.Empty<TextureDefinition>()),
            _ => throw new ArgumentException($"Unsupported uniform type: {type}"),
        };
    }
}