
namespace FantaniaLib;

[BindingScript]
public enum TextureTypes
{
    None,
    Image,
    Atlas,
    // Gradient,
    // Noise,
    Gpu,
}

public struct TextureDefinition : IEquatable<TextureDefinition>
{
    public static readonly TextureDefinition None = new TextureDefinition();

    public TextureTypes TextureType;
    public TextureParameterUnion TextureParameters;

    public static TextureDefinition CreateImageDefinition(string imagePath)
    {
        return new TextureDefinition
        {
            TextureType = TextureTypes.Image,
            TextureParameters = new TextureParameterUnion
            {
                ImageParams = new ImageParameter
                {
                    ImagePath = imagePath,
                },
            },
        };
    }

    public static TextureDefinition CreateAtlasDefinition(string atlasPath, string frameKey)
    {
        return new TextureDefinition
        {
            TextureType = TextureTypes.Atlas,
            TextureParameters = new TextureParameterUnion
            {
                AtlasParams = new AtlasParameter
                {
                    AtlasPath = atlasPath,
                    FrameKey = frameKey,
                },
            },
        };
    }

    public static TextureDefinition CreateGpuDefinition(int texId)
    {
        return new TextureDefinition
        {
            TextureType = TextureTypes.Gpu,
            TextureParameters = new TextureParameterUnion
            {
                GpuParams = new GpuParameter
                {
                    TextureID = texId,
                },
            },
        };
    }

    public TextureDefinition()
    {
        TextureType = TextureTypes.None;
    }

    public ITexture2D? ToTexture(string rootFolder)
    {
        if (TextureType == TextureTypes.None || TextureType == TextureTypes.Gpu) return null;
        if (!_textureCache.TryGetValue(ToString(), out ITexture2D? tex))
        {
            switch (TextureType)
            {
                case TextureTypes.Image:
                    string imgPath = TextureParameters.ImageParams.ImagePath;
                    if (imgPath.StartsWith("avares://") && AvaloniaHelper.HasAsset(imgPath))
                    {
                        tex = new LocalTexture2D(imgPath);
                    }
                    else
                    {
                        imgPath = Path.Combine(rootFolder, TextureParameters.ImageParams.ImagePath);
                        if (File.Exists(imgPath))
                        {
                            tex = new LocalTexture2D(imgPath);
                        }
                    }
                    break;
                case TextureTypes.Atlas:
                    string atlasPath = TextureParameters.AtlasParams.AtlasPath;
                    if (atlasPath.StartsWith("avares://") && AvaloniaHelper.HasAsset(atlasPath))
                    {
                        SpriteAtlas atlas = new SpriteAtlas(atlasPath);
                        if (atlas.IsValid)
                            tex = new AtlasTexture2D(atlas, TextureParameters.AtlasParams.FrameKey);
                    }
                    else
                    {
                        atlasPath = Path.Combine(rootFolder, TextureParameters.AtlasParams.AtlasPath);
                        if (File.Exists(atlasPath))
                        {
                            SpriteAtlas atlas = new SpriteAtlas(atlasPath);
                            if (atlas.IsValid)
                                tex = new AtlasTexture2D(atlas, TextureParameters.AtlasParams.FrameKey);
                        }
                    }
                    break;
            }
            _textureCache.Add(ToString(), tex);
        }
        return tex;
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
                case TextureTypes.Gpu:
                    return lhs.TextureParameters.GpuParams.Equals(rhs.TextureParameters.GpuParams);
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
            TextureTypes.Gpu => TextureParameters.GpuParams.ToString(),
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

    static Dictionary<string, ITexture2D?> _textureCache = new Dictionary<string, ITexture2D?>(128);
}