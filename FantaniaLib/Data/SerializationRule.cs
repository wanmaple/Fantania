using System.Data;
using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public interface IFieldCastRule
{
    string CastTo(object? fieldVal);
    object? CastFrom(string casted);
}

public class SerializationRule
{
    private class DefaultBooleanCast : IFieldCastRule
    {
        public object? CastFrom(string casted)
        {
            int num = int.Parse(casted);
            return num > 0;
        }

        public string CastTo(object? fieldVal)
        {
            bool b = (bool)fieldVal!;
            return b ? "1" : "0";
        }
    }

    private class DefaultIntegerCast : IFieldCastRule
    {
        public object? CastFrom(string casted)
        {
            return int.Parse(casted);
        }

        public string CastTo(object? fieldVal)
        {
            int num = (int)fieldVal!;
            return num.ToString();
        }
    }

    private class DefaultFloatCast : IFieldCastRule
    {
        public object? CastFrom(string casted)
        {
            return float.Parse(casted);
        }

        public string CastTo(object? fieldVal)
        {
            float num = (float)fieldVal!;
            return num.ToString("F2");
        }
    }

    private class DefaultStringCast : IFieldCastRule
    {
        public object? CastFrom(string casted)
        {
            return casted;
        }

        public string CastTo(object? fieldVal)
        {
            string str = (string)fieldVal!;
            return str;
        }
    }

    private class DefaultVector2Cast : IFieldCastRule
    {
        public object? CastFrom(string casted)
        {
            var ary = casted.Split(',');
            return new Vector2(float.Parse(ary[0]), float.Parse(ary[1]));
        }

        public string CastTo(object? fieldVal)
        {
            Vector2 vec = (Vector2)fieldVal!;
            return $"{vec.X.ToString("F2")},{vec.Y.ToString("F2")}";
        }
    }

    private class DefaultColorCast : IFieldCastRule
    {
        public object? CastFrom(string casted)
        {
            Color color = Color.Parse(casted);
            return color.ToVector4();
        }

        public string CastTo(object? fieldVal)
        {
            Vector4 vec = (Vector4)fieldVal!;
            return vec.ToColor().ToHex();
        }
    }

    private class DefaultTextureCast : IFieldCastRule
    {
        public object? CastFrom(string casted)
        {
            if (string.IsNullOrEmpty(casted))
                return TextureDefinition.None;
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

        public string CastTo(object? fieldVal)
        {
            TextureDefinition texDef = (TextureDefinition)fieldVal!;
            return texDef.ToString()!;
        }
    }

    public static readonly SerializationRule DEFAULT = new SerializationRule();

    public SerializationRule()
    {
        SetFieldCast(FieldTypes.Boolean, new DefaultBooleanCast());
        SetFieldCast(FieldTypes.Integer, new DefaultIntegerCast());
        SetFieldCast(FieldTypes.Float, new DefaultFloatCast());
        SetFieldCast(FieldTypes.String, new DefaultStringCast());
        SetFieldCast(FieldTypes.Vector2, new DefaultVector2Cast());
        SetFieldCast(FieldTypes.Color, new DefaultColorCast());
        SetFieldCast(FieldTypes.Texture, new DefaultTextureCast());
    }

    public string CastTo(FieldTypes fieldType, object? fieldValue)
    {
        if (_fieldCasts.TryGetValue(fieldType, out var cast))
        {
            return cast.CastTo(fieldValue);
        }
        throw new DataException($"No cast rule for {fieldType} was setup.");
    }

    public object? CastFrom(FieldTypes fieldType, string castedStr)
    {
        if (_fieldCasts.TryGetValue(fieldType, out var cast))
        {
            return cast.CastFrom(castedStr);
        }
        throw new DataException($"No cast rule for {fieldType} was setup.");
    }

    public void SetFieldCast(FieldTypes fieldType, IFieldCastRule rule)
    {
        _fieldCasts[fieldType] = rule;
    }

    Dictionary<FieldTypes, IFieldCastRule> _fieldCasts = new Dictionary<FieldTypes, IFieldCastRule>(32);
}