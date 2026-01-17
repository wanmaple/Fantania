using System.Collections;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;

namespace FantaniaLib;

internal class TextureDefIsSomeTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return false;
        if (value is not TextureDefinition def) return false;
        if (parameter is not TextureTypes type) return false;
        return def.TextureType == type;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class TextureDef2TextureInfoConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        object? value = _cvt.Convert(values, targetType, parameter, culture);
        if (value != AvaloniaProperty.UnsetValue && value != null)
            return value.ToString();
        return AvaloniaProperty.UnsetValue;
    }

    TextureDef2Texture2DConverter _cvt = new TextureDef2Texture2DConverter();
}

internal class TextureDef2Texture2DConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not TextureDefinition def) return AvaloniaProperty.UnsetValue;
        if (values[1] is not string rootFolder) return AvaloniaProperty.UnsetValue;
        return def.ToTexture(rootFolder);
    }
}

internal class TextureDef2AtlasFramesConverter : IMultiValueConverter
{
    internal class FrameOptions : IEnumerable<string>
    {
        public required IReadOnlyCollection<string> Frames { get; set; }
        public required string SelectedFrame { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            return Frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not TextureDefinition def) return AvaloniaProperty.UnsetValue;
        if (values[1] is not string rootFolder) return AvaloniaProperty.UnsetValue;
        if (def.TextureType != TextureTypes.Atlas) return AvaloniaProperty.UnsetValue;
        string atlasPath = Path.Combine(rootFolder, def.TextureParameters.AtlasParams.AtlasPath);
        if (File.Exists(atlasPath))
        {
            SpriteAtlas atlas = new SpriteAtlas(atlasPath);
            if (atlas.IsValid)
                return new FrameOptions
                {
                    Frames = atlas.Keys,
                    SelectedFrame = def.TextureParameters.AtlasParams.FrameKey,
                };
        }
        return AvaloniaProperty.UnsetValue;
    }
}

public partial class TextureBox : UserControl
{
    IEditableField? Field => DataContext as IEditableField;

    public TextureBox()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        topLevel.RequestAnimationFrame(OnTick);
    }

    void OnTick(TimeSpan dt)
    {
        if (Field != null)
        {
            object? value = obTexType.Value;
            TextureTypes texType = (TextureTypes)value!;
            var def = BuildTextureDef(texType);
            Field.FieldValue = def;
        }
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    TextureDefinition BuildTextureDef(TextureTypes texType)
    {
        var texParams = new TextureParameterUnion();
        switch (texType)
        {
            case TextureTypes.Image:
                texParams.ImageParams = new ImageParameter
                {
                    ImagePath = fsbImage.Path,
                };
                break;
            case TextureTypes.Atlas:
                texParams.AtlasParams = new AtlasParameter
                {
                    AtlasPath = fsbAtlas.Path,
                    FrameKey = (string)cbFrameKeys.SelectedItem!,
                };
                break;
        }
        return new TextureDefinition
        {
            TextureType = texType,
            TextureParameters = texParams,
        };
    }

    void AtlasSelectBox_FileSelected(object? sender, FileSelectedEventArgs e)
    {
        string path = Field!.Workspace.GetAbsolutePath(e.FilePath);
        SpriteAtlas atlas = new SpriteAtlas(path);
        if (atlas.IsValid)
        {
            cbFrameKeys.SelectedItem = atlas.Keys.First();
        }
    }
}