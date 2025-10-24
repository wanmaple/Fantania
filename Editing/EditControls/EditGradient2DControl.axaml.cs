using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace Fantania;

public partial class EditGradient2DControl : UserControl, ICustomHitTest
{
    public static readonly StyledProperty<IEditableProperty> GradientProperty = AvaloniaProperty.Register<EditGradient2DControl, IEditableProperty>(nameof(Gradient), defaultValue: null);
    public IEditableProperty Gradient
    {
        get => GetValue(GradientProperty);
        set => SetValue(GradientProperty, value);
    }

    public static readonly StyledProperty<IEditableProperty> ShapeProperty = AvaloniaProperty.Register<EditGradient2DControl, IEditableProperty>(nameof(Shape), defaultValue: null);
    public IEditableProperty Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public EditGradient2DControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _gradientRenderer = new GradientRenderer(preview.ColorBufferWidth, preview.ColorBufferHeight);
        _gradientRenderer.RefreshDurationInMs = 0;
        preview.AddRenderer(_gradientRenderer);
        OnGradientChanged(EditableProperty.EditValue);
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
        EditableProperty.ValueChanged += OnGradientChanged;
        Type type = typeof(Gradient2D);
        Gradient = new TempEditableProperty(_gradient, type.GetProperty("Gradient"), new EditGradient1DAttribute());
        Shape = new TempEditableProperty(_gradient, type.GetProperty("Shape"), new EditEnumAttribute());
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Vector toStart = e.GetPosition(start);
        Vector toEnd = e.GetPosition(end);
        if (toStart.X > 0.0 && toStart.X < start.Width && toStart.Y > 0.0 && toStart.Y < start.Height)
        {
            _editingBegin = true;
            _editingEnd = false;
        }
        else if (toEnd.X > 0.0 && toEnd.X < end.Width && toEnd.Y > 0.0 && toEnd.Y < end.Height)
        {
            _editingEnd = true;
            _editingBegin = false;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _editingBegin = _editingEnd = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_editingBegin || _editingEnd)
        {
            Vector relativePos = e.GetPosition(preview);
            relativePos /= 256.0;
            relativePos = MathHelper.Clamp(relativePos, Vector.Zero, Vector.One);
            if (_editingBegin)
            {
                _gradient.BeginAnchor = new System.Numerics.Vector2((float)relativePos.X, (float)relativePos.Y);
            }
            else
            {
                _gradient.EndAnchor = new System.Numerics.Vector2((float)relativePos.X, (float)relativePos.Y);
            }
        }
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _gradient.Gradient = (Gradient1D)Gradient.EditValue;
            _gradient.Shape = (GradientShapes)Shape.EditValue;
            if (EditableProperty != null && !EditableProperty.EditValue.Equals(_gradient))
            {
                EditableProperty.EditValue = _gradient.Clone();
            }
            if (EditableProperty != null)
                _gradientRenderer.Gradient = (Gradient2D)EditableProperty.EditValue;
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    void OnGradientChanged(object gradient)
    {
        if (EditableProperty == null) return;
        _gradient = ((Gradient2D)EditableProperty.EditValue).Clone();
        if (Gradient != null)
        {
            Gradient.EditValue = _gradient.Gradient;
        }
        if (Shape != null)
        {
            Shape.EditValue = _gradient.Shape;
        }
    }

    public bool HitTest(Point point)
    {
        return true;
    }

    Gradient2D _gradient;
    GradientRenderer _gradientRenderer;
    bool _editingBegin, _editingEnd;
}