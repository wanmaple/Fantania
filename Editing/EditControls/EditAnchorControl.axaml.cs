using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Fantania;

public class CoordinateXConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        double mul = parameter == null ? 1.0 : System.Convert.ToDouble(parameter);
        if (value is Vector v1)
        {
            return v1.X * mul;
        }
        else if (value is System.Numerics.Vector2 v2)
        {
            return v2.X * (float)mul;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CoordinateYConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        double mul = parameter == null ? 1.0 : System.Convert.ToDouble(parameter);
        if (value is Vector v1)
        {
            return v1.Y * mul;
        }
        else if (value is System.Numerics.Vector2 v2)
        {
            return v2.Y * (float)mul;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class EditAnchorControl : UserControl
{
    public IEditableProperty Property => DataContext as IEditableProperty;

    public EditAnchorControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vec2Type = Property.EditValue.GetType();
    }

    public void SetTopLeft()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 0.0f, 0.0f, });
    }

    public void SetTopCenter()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 0.5f, 0.0f, });
    }

    public void SetTopRight()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 1.0f, 0.0f, });
    }

    public void SetCenterLeft()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 0.0f, 0.5f, });
    }

    public void SetCenter()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 0.5f, 0.5f, });
    }

    public void SetCenterRight()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 1.0f, 0.5f, });
    }

    public void SetBottomLeft()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 0.0f, 1.0f, });
    }

    public void SetBottomCenter()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 0.5f, 1.0f, });
    }

    public void SetBottomRight()
    {
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { 1.0f, 1.0f, });
    }

    void Canvas_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        Canvas canvas = sender as Canvas;
        Point pos = e.GetPosition(canvas);
        double anchorX = pos.X / canvas.Width;
        double anchorY = pos.Y / canvas.Height;
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { (float)anchorX, (float)anchorY, });
        _pressing = true;
    }

    void Canvas_PointerMoved(object sender, PointerEventArgs e)
    {
        if (!_pressing) return;
        Canvas canvas = sender as Canvas;
        Point pos = e.GetPosition(canvas);
        double anchorX = pos.X / canvas.Width;
        double anchorY = pos.Y / canvas.Height;
        anchorX = Math.Clamp(anchorX, 0.0, 1.0);
        anchorY = Math.Clamp(anchorY, 0.0, 1.0);
        Property.EditValue = Activator.CreateInstance(_vec2Type, new object[] { (float)anchorX, (float)anchorY, });
    }

    void Canvas_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _pressing = false;
    }

    bool _pressing;
    Type _vec2Type;
}