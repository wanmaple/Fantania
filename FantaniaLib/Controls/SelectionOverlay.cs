using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FantaniaLib;

public class SelectionOverlay : Control, IObserver<Rect>, IObserver<double>
{
    public static readonly StyledProperty<double> PulsePhaseProperty = AvaloniaProperty.Register<SelectionOverlay, double>(nameof(PulsePhase), defaultValue: 0.0);
    public double PulsePhase
    {
        get => GetValue(PulsePhaseProperty);
        set => SetValue(PulsePhaseProperty, value);
    }

    public static readonly StyledProperty<Rect> SelectionBoundsProperty = AvaloniaProperty.Register<SelectionOverlay, Rect>(nameof(SelectionBounds), defaultValue: new Rect());
    public Rect SelectionBounds
    {
        get => GetValue(SelectionBoundsProperty);
        set => SetValue(SelectionBoundsProperty, value);
    }

    public static readonly StyledProperty<Color> SelectionColorProperty = AvaloniaProperty.Register<SelectionOverlay, Color>(nameof(SelectionColor), defaultValue: Colors.Cyan);
    public Color SelectionColor
    {
        get => GetValue(SelectionColorProperty);
        set => SetValue(SelectionColorProperty, value);
    }

    public static readonly StyledProperty<bool> IsAnimatedProperty = AvaloniaProperty.Register<SelectionOverlay, bool>(nameof(IsAnimated), defaultValue: true);
    public bool IsAnimated
    {
        get => GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }
    
    private IDisposable? _animationTimer;
    private DateTime _lastUpdateTime = DateTime.Now;
    
    public SelectionOverlay()
    {
        this.GetObservable(SelectionBoundsProperty).Subscribe(this);
        this.GetObservable(PulsePhaseProperty).Subscribe(this);
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        StartAnimation();
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopAnimation();
    }
    
    private void StartAnimation()
    {
        _animationTimer = Avalonia.Threading.DispatcherTimer.Run(() =>
        {
            var now = DateTime.Now;
            var delta = (now - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = now;
            PulsePhase = (PulsePhase + delta * 2.0) % (Math.PI * 2);
            return true;
        }, TimeSpan.FromMilliseconds(16)); // 60 FPS
    }
    
    private void StopAnimation()
    {
        _animationTimer?.Dispose();
        _animationTimer = null;
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (SelectionBounds.Width <= 0.0 || SelectionBounds.Height <= 0.0)
            return;
        DrawSelectionBox(context);
    }
    
    private void DrawSelectionBox(DrawingContext context)
    {
        var bounds = SelectionBounds;
        var color = SelectionColor;
        double pulse = (Math.Sin(PulsePhase) + 1) * 0.5;
        double alpha = IsAnimated ? 0.2 + pulse * 0.2 : 0.2;
        double animatedWidth = IsAnimated ? 2.0 + pulse * 1.0 : 2.0;
        var fillBrush = new SolidColorBrush(color.WithAlpha(MathHelper.ToByte((float)alpha)));
        var borderBrush = new SolidColorBrush(color);
        var pen = new Pen(borderBrush, animatedWidth);
        context.FillRectangle(fillBrush, bounds);
        context.DrawRectangle(pen, bounds);
        DrawCornerHandles(context, bounds, color, pulse);
    }
    
    private void DrawCornerHandles(DrawingContext context, Rect bounds, Color color, double pulse)
    {
        double handleSize = IsAnimated ? 8 + pulse * 2 : 8;
        double halfHandle = handleSize / 2;
        var handleBrush = new SolidColorBrush(color);
        var corners = new[]
        {
            new Point(bounds.X, bounds.Y), 
            new Point(bounds.Right, bounds.Y),
            new Point(bounds.Right, bounds.Bottom),
            new Point(bounds.X, bounds.Bottom),
        };
        foreach (var corner in corners)
        {
            var handleRect = new Rect(
                corner.X - halfHandle,
                corner.Y - halfHandle,
                handleSize,
                handleSize);
            context.FillRectangle(handleBrush, handleRect);
        }
    }

    void IObserver<Rect>.OnCompleted()
    {
    }

    void IObserver<Rect>.OnError(Exception error)
    {
    }

    void IObserver<Rect>.OnNext(Rect value)
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
}