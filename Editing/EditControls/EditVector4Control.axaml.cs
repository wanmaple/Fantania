using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Fantania;

public partial class EditVector4Control : UserControl
{
    public static readonly StyledProperty<double> XProperty = AvaloniaProperty.Register<EditVector4Control, double>(nameof(X), defaultValue: 0.0);
    public double X
    {
        get => GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public static readonly StyledProperty<double> YProperty = AvaloniaProperty.Register<EditVector4Control, double>(nameof(Y), defaultValue: 0.0);
    public double Y
    {
        get => GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public static readonly StyledProperty<double> ZProperty = AvaloniaProperty.Register<EditVector4Control, double>(nameof(Z), defaultValue: 0.0);
    public double Z
    {
        get => GetValue(ZProperty);
        set => SetValue(ZProperty, value);
    }

    public static readonly StyledProperty<double> WProperty = AvaloniaProperty.Register<EditVector4Control, double>(nameof(W), defaultValue: 0.0);
    public double W
    {
        get => GetValue(WProperty);
        set => SetValue(WProperty, value);
    }

    IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public EditVector4Control()
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
        Vector4 vec = (Vector4)value;
        X = vec.X;
        Y = vec.Y;
        Z = vec.Z;
        W = vec.W;
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (EditableProperty != null)
                EditableProperty.EditValue = new Vector4((float)X, (float)Y, (float)Z, (float)W);
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }
}