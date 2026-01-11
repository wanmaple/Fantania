
namespace FantaniaLib;

public enum TextureTypes
{
    None,
    Image,
    Atlas,
    // Gradient,
    // Noise,
}

public struct TextureDefinition : IEquatable<TextureDefinition>
{
    public static readonly TextureDefinition None = new TextureDefinition();

    public TextureTypes TextureType;
    public TextureParameterUnion TextureParameters;

    public TextureDefinition()
    {
        TextureType = TextureTypes.None;
    }

    public static bool operator ==(TextureDefinition lhs, TextureDefinition rhs)
    {
        if (lhs.TextureType == rhs.TextureType)
        {
            switch (lhs.TextureType)
            {
                case TextureTypes.None:
                    return true;
                case TextureTypes.Image:
                    return lhs.TextureParameters.ImageParams.Equals(rhs.TextureParameters.ImageParams);
                case TextureTypes.Atlas:
                    return lhs.TextureParameters.AtlasParams.Equals(rhs.TextureParameters.AtlasParams);
            }
        }
        return false;
    }

    public static bool operator !=(TextureDefinition lhs, TextureDefinition rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(TextureDefinition other)
    {
        return this == other;
    }

    public override string ToString()
    {
        return $"{(int)TextureType},{TextureType switch
        {
            TextureTypes.Image => TextureParameters.ImageParams.ToString(),
            TextureTypes.Atlas => TextureParameters.AtlasParams.ToString(),
            _ => 0,
        }}";
    }

    public override bool Equals(object? obj)
    {
        return obj is TextureDefinition && Equals((TextureDefinition)obj);
    }

    public override int GetHashCode()
    {
        return (TextureType.GetHashCode() * 397) ^ TextureParameters.GetHashCode();
    }
}