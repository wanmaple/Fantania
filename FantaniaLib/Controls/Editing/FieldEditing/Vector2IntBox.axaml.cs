using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FantaniaLib;

public partial class Vector2IntBox : UserControl
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

    public Vector2IntBox()
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
                RangeMinimum = int.Parse(ary[0]);
                if (ary.Length >= 2)
                    RangeMaximum = int.Parse(ary[1]);
                if (ary.Length >= 3)
                    RangeIncrement = int.Parse(ary[2]);
            }
        }
    }

    void OnTick(TimeSpan dt)
    {
        if (Field != null)
        {
            int x = Convert.ToInt32(numX.Value);
            int y = Convert.ToInt32(numY.Value);
            Field.FieldValue = new Vector2Int(x, y);
        }
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }
}