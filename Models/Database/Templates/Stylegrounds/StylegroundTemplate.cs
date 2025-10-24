using System;

namespace Fantania.Models;

public abstract class StylegroundTemplate : DatabaseObject
{
    public abstract Type StylegroundType { get; }

    public override string IconPath => _texture;

    private string _texture = TextureCache.WHITE;
    [EditString(ControlType = typeof(ImagePickerControl)), DatabaseString, Tooltip("TooltipMainTexture")]
    public string MainTexture
    {
        get { return _texture; }
        set
        {
            if (_texture != value)
            {
                OnPropertyChanging(nameof(MainTexture));
                _texture = value;
                OnPropertyChanged(nameof(MainTexture));
                OnPropertyChanged(nameof(IconPath));
            }
        }
    }

    protected StylegroundTemplate()
    {
    }

    public abstract IRenderer CreateRenderer();
    public abstract void UpdateRenderer(IRenderer renderer);
}