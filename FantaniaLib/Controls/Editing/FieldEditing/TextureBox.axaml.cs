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

public partial class TextureBox : UserControl
{
    IEditableField? Field => DataContext as IEditableField;

    public static readonly StyledProperty<string> RootFolderProperty = AvaloniaProperty.Register<TextureBox, string>(nameof(RootFolder), defaultValue: string.Empty);
    public string RootFolder
    {
        get => GetValue(RootFolderProperty);
        set => SetValue(RootFolderProperty, value);
    }

    public static readonly StyledProperty<ITexture2D?> DisplayTextureProperty = AvaloniaProperty.Register<TextureBox, ITexture2D?>(nameof(DisplayTexture), defaultValue: null);
    public ITexture2D? DisplayTexture
    {
        get => GetValue(DisplayTextureProperty);
        set => SetValue(DisplayTextureProperty, value);
    }

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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (Field != null)
        {
            RootFolder = Field.EditInfo.EditParameter;
            DisplayTexture = GenerateTexture2D((TextureDefinition)Field.FieldValue);
        }
    }

    void OnTick(TimeSpan dt)
    {
        if (Field != null)
        {
            object? value = obTexType.Value;
            TextureTypes texType = (TextureTypes)value!;
            var def = BuildTextureDef(texType);
            if (!Field.FieldValue.Equals(def))
            {
                Field.FieldValue = def;
                DisplayTexture = GenerateTexture2D(def);
            }
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
                break;
        }
        return new TextureDefinition
        {
            TextureType = texType,
            TextureParameters = texParams,
        };
    }

    ITexture2D? GenerateTexture2D(TextureDefinition def)
    {
        switch (def.TextureType)
        {
            case TextureTypes.Image:
                string path = Path.Combine(RootFolder, def.TextureParameters.ImageParams.ImagePath);
                if (File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        return new Texture2D(fs);
                    }
                }
                break;
        }
        return null;
    }
}