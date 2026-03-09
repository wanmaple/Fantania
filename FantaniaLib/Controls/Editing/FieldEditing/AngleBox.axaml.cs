using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public class Radian2DegreeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not float rad) return AvaloniaProperty.UnsetValue;
        return (decimal)MathHelper.Radian2Degree(rad);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not decimal deg) return AvaloniaProperty.UnsetValue;
        return MathHelper.Degree2Radian((float)deg);
    }
}

public partial class AngleBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    public static readonly StyledProperty<float> RangeMinimumProperty = AvaloniaProperty.Register<AngleBox, float>(nameof(RangeMinimum), defaultValue: -180.0f);
    public float RangeMinimum
    {
        get => GetValue(RangeMinimumProperty);
        set => SetValue(RangeMinimumProperty, value);
    }

    public static readonly StyledProperty<float> RangeMaximumProperty = AvaloniaProperty.Register<AngleBox, float>(nameof(RangeMaximum), defaultValue: 180.0f);
    public float RangeMaximum
    {
        get => GetValue(RangeMaximumProperty);
        set => SetValue(RangeMaximumProperty, value);
    }

    public static readonly StyledProperty<float> RangeIncrementProperty = AvaloniaProperty.Register<AngleBox, float>(nameof(RangeIncrement), defaultValue: 1.0f);
    public float RangeIncrement
    {
        get => GetValue(RangeIncrementProperty);
        set => SetValue(RangeIncrementProperty, value);
    }

    public AngleBox()
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