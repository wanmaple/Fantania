using Avalonia;
using Avalonia.Controls;

namespace FantaniaLib;

public partial class IntegerBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    public static readonly StyledProperty<int> RangeMinimumProperty = AvaloniaProperty.Register<IntegerBox, int>(nameof(RangeMinimum), defaultValue: int.MinValue);
    public int RangeMinimum
    {
        get => GetValue(RangeMinimumProperty);
        set => SetValue(RangeMinimumProperty, value);
    }

    public static readonly StyledProperty<int> RangeMaximumProperty = AvaloniaProperty.Register<IntegerBox, int>(nameof(RangeMaximum), defaultValue: int.MaxValue);
    public int RangeMaximum
    {
        get => GetValue(RangeMaximumProperty);
        set => SetValue(RangeMaximumProperty, value);
    }

    public static readonly StyledProperty<int> RangeIncrementProperty = AvaloniaProperty.Register<IntegerBox, int>(nameof(RangeIncrement), defaultValue: 1);
    public int RangeIncrement
    {
        get => GetValue(RangeIncrementProperty);
        set => SetValue(RangeIncrementProperty, value);
    }

    public IntegerBox()
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
                RangeMinimum = int.Parse(ary[0]);
                RangeMaximum = int.Parse(ary[1]);
                if (ary.Length >= 3)
                {
                    RangeIncrement = int.Parse(ary[2]);
                }
            }
        }
    }
}