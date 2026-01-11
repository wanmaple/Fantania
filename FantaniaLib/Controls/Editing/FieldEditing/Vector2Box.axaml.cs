using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;

namespace FantaniaLib;

internal class Vector2XConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not Vector2 vec) return AvaloniaProperty.UnsetValue;
        return vec.X;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class Vector2YConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not Vector2 vec) return AvaloniaProperty.UnsetValue;
        return vec.Y;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class Vector2Box : UserControl
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

    public Vector2Box()
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

    void OnTick(TimeSpan dt)
    {
        if (Field != null)
        {
            float x = Convert.ToSingle(numX.Value);
            float y = Convert.ToSingle(numY.Value);
            Field.FieldValue = new Vector2(x, y);
        }
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }
}