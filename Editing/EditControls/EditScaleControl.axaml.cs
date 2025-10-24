using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace Fantania;

public partial class EditScaleControl : UserControl
{
    IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public static readonly StyledProperty<double> XProperty = AvaloniaProperty.Register<EditVector2Control, double>(nameof(X), defaultValue: 0.0);
    public double X
    {
        get => GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public static readonly StyledProperty<double> YProperty = AvaloniaProperty.Register<EditVector2Control, double>(nameof(Y), defaultValue: 0.0);
    public double Y
    {
        get => GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public EditScaleControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        OnValueChanged(EditableProperty.EditValue);
        // 没辙，结构体没办法同步到对象上，这里在每一帧设置一次数值。
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
        EditableProperty.RegisterPropertyChanged(OnValueChanged);
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        EditableProperty.UnRegisterPropertyChanged(OnValueChanged);
    }

    void OnValueChanged(object value)
    {
        Vector vec = (Vector)value;
        X = vec.X;
        Y = vec.Y;
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (EditableProperty != null)
                EditableProperty.EditValue = new Vector(X, Y);
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    void OnXValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        double oldX = Convert.ToDouble(e.OldValue);
        double x = Convert.ToDouble(e.NewValue);
        Vector vec = (Vector)EditableProperty.EditValue;
        if (_chained && oldX != 0.0)
        {
            double y = vec.Y;
            double ratio = y / oldX;
            nudY.ValueChanged -= OnYValueChanged;
            Y = X * ratio;
            nudY.ValueChanged += OnYValueChanged;
        }
        else
        {
            EditableProperty.EditValue = new Vector(x, vec.Y);
        }
    }

    void OnYValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        double oldY = Convert.ToDouble(e.OldValue);
        double y = Convert.ToDouble(e.NewValue);
        Vector vec = (Vector)EditableProperty.EditValue;
        if (_chained && oldY != 0.0)
        {
            double x = vec.X;
            double ratio = x / oldY;
            nudX.ValueChanged -= OnXValueChanged;
            X = Y * ratio;
            nudX.ValueChanged += OnXValueChanged;
        }
        else
        {
            EditableProperty.EditValue = new Vector(vec.X, y);
        }
    }

    void ButtonChain_Click(object? sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        _chained = !_chained;
        if (_chained)
            button.Background = Brushes.DarkGray;
        else
            button.Background = ThemeHelper.BackgroundBrush;
    }

    bool _chained = false;
}