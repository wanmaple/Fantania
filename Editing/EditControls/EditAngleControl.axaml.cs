using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Fantania;

public partial class EditAngleControl : UserControl
{
    IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public static readonly StyledProperty<double> AngleProperty = AvaloniaProperty.Register<EditAngleControl, double>(nameof(Angle), defaultValue: 0.0);
    public double Angle
    {
        get => GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    public EditAngleControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        OnValueChanged(EditableProperty.EditValue);
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

    void OnValueChanged(object val)
    {
        Angle = (double)val;
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (EditableProperty != null)
                EditableProperty.EditValue = Angle;
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }
}