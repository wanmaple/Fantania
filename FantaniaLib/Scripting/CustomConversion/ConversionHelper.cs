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
            FieldTypes.Direction3D => val.GetObjectOrDefault(Vector3.UnitZ),
            FieldTypes.Texture => val.GetObjectOrDefault(TextureDefinition.None),
            FieldTypes.GroupReference => val.GetObjectOrDefault(GroupReference.None),
            FieldTypes.TypeReference => val.GetObjectOrDefault(TypeReference.None),
            FieldTypes.BooleanArray => val.GetObjectOrDefault(new FantaniaArray<bool>()),
            FieldTypes.IntegerArray => val.GetObjectOrDefault(new FantaniaArray<int>()),
            FieldTypes.FloatArray => val.GetObjectOrDefault(new FantaniaArray<float>()),
            FieldTypes.StringArray => val.GetObjectOrDefault(new FantaniaArray<string>()),
            FieldTypes.Vector2Array => val.GetObjectOrDefault(new FantaniaArray<Vector2>()),
            FieldTypes.Vector2IntArray => val.GetObjectOrDefault(new FantaniaArray<Vector2Int>()),
            FieldTypes.Vector3Array => val.GetObjectOrDefault(new FantaniaArray<Vector3>()),
            FieldTypes.ColorArray => val.GetObjectOrDefault(new FantaniaArray<Vector4>()),
            FieldTypes.Direction3DArray => val.GetObjectOrDefault(new FantaniaArray<Vector3>()),
            FieldTypes.TextureArray => val.GetObjectOrDefault(new FantaniaArray<TextureDefinition>()),
            FieldTypes.GroupReferenceArray => val.GetObjectOrDefault(new FantaniaArray<GroupReference>()),
            FieldTypes.TypeReferenceArray => val.GetObjectOrDefault(new FantaniaArray<TypeReference>()),
            _ => null,
        };
    }

    public static DynValue FieldTypeToDynValue(Script env, FieldTypes type, object value)
    {
        return type switch
        {
            FieldTypes.Boolean => DynValue.NewBoolean((bool)value),
            FieldTypes.Integer => DynValue.NewNumber((int)value),
            FieldTypes.Float => DynValue.NewNumber((float)value),
            FieldTypes.String => DynValue.NewString((string)value),
            FieldTypes.Vector2 => DynValue.FromObject(env, (Vector2)value),
            FieldTypes.Vector2Int => DynValue.FromObject(env, (Vector2Int)value),
            FieldTypes.Vector3 => DynValue.FromObject(env, (Vector3)value),
            FieldTypes.Color => DynValue.FromObject(env, (Vector4)value),
            FieldTypes.Direction3D => DynValue.FromObject(env, (Vector3)value),
            FieldTypes.Texture => DynValue.FromObject(env, (TextureDefinition)value),
            FieldTypes.GroupReference => DynValue.FromObject(env, (GroupReference)value),
            FieldTypes.TypeReference => DynValue.FromObject(env, (TypeReference)value),
            FieldTypes.BooleanArray => DynValue.FromObject(env, (FantaniaArray<bool>)value),
            FieldTypes.IntegerArray => DynValue.FromObject(env, (FantaniaArray<int>)value),
            FieldTypes.FloatArray => DynValue.FromObject(env, (FantaniaArray<float>)value),
            FieldTypes.StringArray => DynValue.FromObject(env, (FantaniaArray<string>)value),
            FieldTypes.Vector2Array => DynValue.FromObject(env, (FantaniaArray<Vector2>)value),
            FieldTypes.Vector2IntArray => DynValue.FromObject(env, (FantaniaArray<Vector2Int>)value),
            FieldTypes.Vector3Array => DynValue.FromObject(env, (FantaniaArray<Vector3>)value),
            FieldTypes.ColorArray => DynValue.FromObject(env, (FantaniaArray<Vector4>)value),
            FieldTypes.Direction3DArray => DynValue.FromObject(env, (FantaniaArray<Vector3>)value),
            FieldTypes.TextureArray => DynValue.FromObject(env, (FantaniaArray<TextureDefinition>)value),
            FieldTypes.GroupReferenceArray => DynValue.FromObject(env, (FantaniaArray<GroupReference>)value),
            FieldTypes.TypeReferenceArray => DynValue.FromObject(env, (FantaniaArray<TypeReference>)value),
            _ => DynValue.Nil,
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
        if (type == typeof(Direction3D)) return FieldTypes.Direction3D;
        if (type == typeof(TextureDefinition)) return FieldTypes.Texture;
        if (type == typeof(GroupReference)) return FieldTypes.GroupReference;
        if (type == typeof(TypeReference)) return FieldTypes.TypeReference;
        if (type == typeof(FantaniaArray<bool>)) return FieldTypes.BooleanArray;
        if (type == typeof(FantaniaArray<int>)) return FieldTypes.IntegerArray;
        if (type == typeof(FantaniaArray<float>)) return FieldTypes.FloatArray;
        if (type == typeof(FantaniaArray<string>)) return FieldTypes.StringArray;
        if (type == typeof(FantaniaArray<Vector2>)) return FieldTypes.Vector2Array;
        if (type == typeof(FantaniaArray<Vector2Int>)) return FieldTypes.Vector2IntArray;
        if (type == typeof(FantaniaArray<Vector3>)) return FieldTypes.Vector3Array;
        if (type == typeof(FantaniaArray<Vector4>)) return FieldTypes.ColorArray;
        if (type == typeof(FantaniaArray<Direction3D>)) return FieldTypes.Direction3DArray;
        if (type == typeof(FantaniaArray<TextureDefinition>)) return FieldTypes.TextureArray;
        if (type == typeof(FantaniaArray<GroupReference>)) return FieldTypes.GroupReferenceArray;
        if (type == typeof(FantaniaArray<TypeReference>)) return FieldTypes.TypeReferenceArray;
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