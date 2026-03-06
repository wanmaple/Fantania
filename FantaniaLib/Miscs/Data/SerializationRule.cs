using System.Data;
using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public interface IFieldCastRule
{
    string CastTo(object? fieldVal, object instance);
    object? CastFrom(string casted, object instance);
}

public class SerializationRule
{
    private class DefaultBooleanCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return false;
            int num = int.Parse(casted);
            return num > 0;
        }

        public string CastTo(object? fieldVal, object instance)
        {
            bool b = (bool)fieldVal!;
            return b ? "1" : "0";
        }
    }

    private class DefaultIntegerCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return 0;
            return int.Parse(casted);
        }

        public string CastTo(object? fieldVal, object instance)
        {
            int num = (int)fieldVal!;
            return num.ToString();
        }
    }

    private class DefaultFloatCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return 0.0f;
            return float.Parse(casted);
        }

        public string CastTo(object? fieldVal, object instance)
        {
            float num = (float)fieldVal!;
            return num.ToString("F2");
        }
    }

    private class DefaultStringCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            return casted;
        }

        public string CastTo(object? fieldVal, object instance)
        {
            string str = (string)fieldVal!;
            return str;
        }
    }

    private class DefaultVector2Cast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return Vector2.Zero;
            var ary = casted.Split(',');
            return new Vector2(float.Parse(ary[0]), float.Parse(ary[1]));
        }

        public string CastTo(object? fieldVal, object instance)
        {
            Vector2 vec = (Vector2)fieldVal!;
            return $"{vec.X.ToString("F2")},{vec.Y.ToString("F2")}";
        }
    }

    private class DefaultVector2IntCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return Vector2Int.Zero;
            var ary = casted.Split(',');
            return new Vector2Int(int.Parse(ary[0]), int.Parse(ary[1]));
        }

        public string CastTo(object? fieldVal, object instance)
        {
            Vector2Int vec = (Vector2Int)fieldVal!;
            return $"{vec.X},{vec.Y}";
        }
    }

    private class DefaultVector3Cast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return Vector3.Zero;
            var ary = casted.Split(',');
            return new Vector3(float.Parse(ary[0]), float.Parse(ary[1]), float.Parse(ary[2]));
        }

        public string CastTo(object? fieldVal, object instance)
        {
            Vector3 vec = (Vector3)fieldVal!;
            return $"{vec.X.ToString("F2")},{vec.Y.ToString("F2")},{vec.Z.ToString("F2")}";
        }
    }

    private class DefaultColorCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return Vector4.One;
            Color color = Color.Parse(casted);
            return color.ToVector4();
        }

        public string CastTo(object? fieldVal, object instance)
        {
            Vector4 vec = (Vector4)fieldVal!;
            return vec.ToColor().ToHex();
        }
    }

    private class Direction3DCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return new Direction3D();
            var ary = casted.Split(',');
            return new Direction3D
            {
                Azimuth = float.Parse(ary[0]),
                Elevation = float.Parse(ary[1]),
            };
        }

        public string CastTo(object? fieldVal, object instance)
        {
            Direction3D dir = (Direction3D)fieldVal!;
            return $"{dir.Azimuth.ToString("F2")},{dir.Elevation.ToString("F2")}";
        }
    }

    private class DefaultTextureCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            if (string.IsNullOrEmpty(casted)) return TextureDefinition.None;
            string[] ary = casted.Split(',');
            TextureTypes texType = (TextureTypes)int.Parse(ary[0]);
            var def = new TextureDefinition
            {
                TextureType = texType,
            };
            if (texType == TextureTypes.Image)
                def.TextureParameters.ImageParams = new ImageParameter
                {
                    ImagePath = ary[1],
                };
            else if (texType == TextureTypes.Atlas)
            {
                int splitIdx = ary[1].LastIndexOf(':');
                string atlasPath = ary[1].Substring(0, splitIdx);
                string frameKey = ary[1].Substring(splitIdx + 1);
                def.TextureParameters.AtlasParams = new AtlasParameter
                {
                    AtlasPath = atlasPath,
                    FrameKey = frameKey,
                };
            }
            return def;
        }

        public string CastTo(object? fieldVal, object instance)
        {
            TextureDefinition texDef = (TextureDefinition)fieldVal!;
            return texDef.ToString()!;
        }
    }

    private class DefaultGroupReferenceCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            string[] ary = casted.Split(',');
            string group = ary[0];
            int id = int.Parse(ary[1]);
            return new GroupReference
            {
                ReferenceGroup = group,
                ReferenceID = id,
            };
        }

        public string CastTo(object? fieldVal, object instance)
        {
            var groupRef = (GroupReference)fieldVal!;
            return groupRef.ToString();
        }
    }

    private class DefaultTypeReferenceCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            string[] ary = casted.Split(',');
            string type = ary[0];
            int id = int.Parse(ary[1]);
            return new TypeReference
            {
                ReferenceType = type,
                ReferenceID = id,
            };
        }

        public string CastTo(object? fieldVal, object instance)
        {
            var typeRef = (TypeReference)fieldVal!;
            return typeRef.ToString();
        }
    }

    private class DefaultEnumCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            string[] ary = casted.Split(',');
            string enumTypeName = ary[0];
            int enumValue = int.Parse(ary[1]);
            Type enumType = Type.GetType(enumTypeName)!;
            return Enum.ToObject(enumType, enumValue);
        }

        public string CastTo(object? fieldVal, object instance)
        {
            Enum enumVal = (Enum)fieldVal!;
            Type enumType = enumVal.GetType();
            return $"{enumType.FullName},{Convert.ToInt32(enumVal)}";
        }
    }

    private class DefaultCustomSerializableCast : IFieldCastRule
    {
        public object? CastFrom(string casted, object instance)
        {
            int index = casted.IndexOf(' ');
            string typeName = casted.Substring(0, index);
            string dataStr = casted.Substring(index + 1);
            Type type = Type.GetType(typeName)!;
            var obj = (ICustomSerializableField)Activator.CreateInstance(type)!;
            obj.DeserializeFromString(dataStr, instance);
            return obj;
        }

        public string CastTo(object? fieldVal, object instance)
        {
            var obj = (ICustomSerializableField)fieldVal!;
            Type objType = obj.GetType();
            return objType.FullName + " " + obj.SerializeToString(instance);
        }
    }

    public static readonly SerializationRule Default = new SerializationRule();

    public SerializationRule()
    {
        SetFieldCast(FieldTypes.Boolean, new DefaultBooleanCast());
        SetFieldCast(FieldTypes.Integer, new DefaultIntegerCast());
        SetFieldCast(FieldTypes.Float, new DefaultFloatCast());
        SetFieldCast(FieldTypes.String, new DefaultStringCast());
        SetFieldCast(FieldTypes.Vector2, new DefaultVector2Cast());
        SetFieldCast(FieldTypes.Vector2Int, new DefaultVector2IntCast());
        SetFieldCast(FieldTypes.Vector3, new DefaultVector3Cast());
        SetFieldCast(FieldTypes.Color, new DefaultColorCast());
        SetFieldCast(FieldTypes.Direction3D, new Direction3DCast());
        SetFieldCast(FieldTypes.Texture, new DefaultTextureCast());
        SetFieldCast(FieldTypes.GroupReference, new DefaultGroupReferenceCast());
        SetFieldCast(FieldTypes.TypeReference, new DefaultTypeReferenceCast());
        SetFieldCast(FieldTypes.Enum, new DefaultEnumCast());
        SetFieldCast(FieldTypes.Custom, new DefaultCustomSerializableCast());
    }

    public string CastTo(FieldTypes fieldType, object? fieldValue, object instance)
    {
        if (_fieldCasts.TryGetValue(fieldType, out var cast))
        {
            return cast.CastTo(fieldValue, instance);
        }
        throw new DataException($"No cast rule for {fieldType} was setup.");
    }

    public object? CastFrom(FieldTypes fieldType, string castedStr, object instance)
    {
        if (_fieldCasts.TryGetValue(fieldType, out var cast))
        {
            return cast.CastFrom(castedStr, instance);
        }
        throw new DataException($"No cast rule for {fieldType} was setup.");
    }

    public void SetFieldCast(FieldTypes fieldType, IFieldCastRule rule)
    {
        _fieldCasts[fieldType] = rule;
    }

    Dictionary<FieldTypes, IFieldCastRule> _fieldCasts = new Dictionary<FieldTypes, IFieldCastRule>(32);
}