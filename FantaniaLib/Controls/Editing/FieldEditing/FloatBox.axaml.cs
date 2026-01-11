using Avalonia;
using Avalonia.Controls;

namespace FantaniaLib;

public partial class FloatBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    public static readonly StyledProperty<float> RangeMinimumProperty = AvaloniaProperty.Register<IntegerBox, float>(nameof(RangeMinimum), defaultValue: float.MinValue);
    public float RangeMinimum
    {
        get => GetValue(RangeMinimumProperty);
        set => SetValue(RangeMinimumProperty, value);
    }

    public static readonly StyledProperty<float> RangeMaximumProperty = AvaloniaProperty.Register<IntegerBox, float>(nameof(RangeMaximum), defaultValue: float.MaxValue);
    public float RangeMaximum
    {
        get => GetValue(RangeMaximumProperty);
        set => SetValue(RangeMaximumProperty, value);
    }

    public static readonly StyledProperty<float> RangeIncrementProperty = AvaloniaProperty.Register<IntegerBox, float>(nameof(RangeIncrement), defaultValue: 0.1f);
    public float RangeIncrement
    {
        get => GetValue(RangeIncrementProperty);
        set => SetValue(RangeIncrementProperty, value);
    }

    public FloatBox()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (Field != null)
        {
            string args = Field.EditInfo.EditParameter;
            if (!string.IsNullOrEmpty(args))
            {
                // "min:max:inc"
                var ary = args.Split(':');
                RangeMinimum = float.Parse(ary[0]);
                RangeMaximum = float.Parse(ary[1]);
                if (ary.Length >= 3)
                {
                    RangeIncrement = float.Parse(ary[2]);
                }
            }
        }
    }
}