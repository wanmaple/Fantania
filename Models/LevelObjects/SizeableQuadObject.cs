using Avalonia;
using Avalonia.OpenGL;

namespace Fantania.Models;

[DisableEditing("Scale")]
public class SizeableQuadObject : QuadObject, ISizeableObject
{
    public override CreateModes CreateMode => CreateModes.Sizeable;
    public override bool IsScaleable => false;

    public double Top => -Anchor.Y * CustomSize.Y + Position.Y;
    public double Bottom => (1.0 - Anchor.Y) * CustomSize.Y + Position.Y;
    public double Left => -Anchor.X * CustomSize.X + Position.X;
    public double Right => (1.0 - Anchor.X) * CustomSize.X + Position.X;

    private Vector _customSize = Vector.Zero;
    [EditVector2(XMinimum = 0.0f, YMinimum = 0.0f, Increment = 4.0f), StandardSerialization(1)]
    public Vector CustomSize
    {
        get { return _customSize; }
        set
        {
            if (_customSize != value)
            {
                OnPropertyChanging(nameof(CustomSize));
                _customSize = value;
                OnPropertyChanged(nameof(CustomSize));
                Size = _customSize;
            }
        }
    }

    public SizeableQuadObject()
    {
    }

    public SizeableQuadObject(DrawTemplate template)
    : base(template)
    {
    }

    public override void OnEnterCanvas(GlInterface gl)
    {
        Size = CustomSize;
    }
}