using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FantaniaLib;

public class SnapOverlay : Control, IObserver<SnapPointShapes>, IObserver<double>, IObserver<Color>
{
    public static readonly StyledProperty<SnapPointShapes> ShapeProperty = AvaloniaProperty.Register<SnapOverlay, SnapPointShapes>(nameof(Shape), defaultValue: SnapPointShapes.Circle);
    public SnapPointShapes Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<SnapOverlay, double>(nameof(Size), defaultValue: 0.0);
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly StyledProperty<Color> ColorProperty = AvaloniaProperty.Register<SnapOverlay, Color>(nameof(Color), defaultValue: Colors.White);
    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public SnapOverlay()
    {
        this.GetObservable(ShapeProperty).Subscribe(this);
        this.GetObservable(SizeProperty).Subscribe(this);
        this.GetObservable(ColorProperty).Subscribe(this);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Shape == SnapPointShapes.Circle)
        {
            DrawCircle(context);
        }
    }

    void DrawCircle(DrawingContext context)
    {
        var brush = new SolidColorBrush(Color);
        var pen = new Pen(brush, 2);
        context.DrawEllipse(null, pen, new Point(), Size, Size);
    }

    void IObserver<SnapPointShapes>.OnCompleted()
    {
    }

    void IObserver<SnapPointShapes>.OnError(Exception error)
    {
    }

    void IObserver<SnapPointShapes>.OnNext(SnapPointShapes value)
    {
        InvalidateVisual();
    }

    void IObserver<double>.OnCompleted()
    {
    }

    void IObserver<double>.OnError(Exception error)
    {
    }

    void IObserver<double>.OnNext(double value)
    {
        InvalidateVisual();
    }

    void IObserver<Color>.OnCompleted()
    {
    }

    void IObserver<Color>.OnError(Exception error)
    {
    }

    void IObserver<Color>.OnNext(Color value)
    {
        InvalidateVisual();
    }
}