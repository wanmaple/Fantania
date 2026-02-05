using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace FantaniaLib;

public class GizmoEventArgs : EventArgs
{
    public TransformGizmoHandles Handle { get; set; }
    public required PointerEventArgs PointerArgs { get; set; }
}

public class TransformGizmoOverlay : Control
{
    public event EventHandler<GizmoEventArgs>? TranslationStart;
    public event EventHandler<GizmoEventArgs>? Translating;
    public event EventHandler<GizmoEventArgs>? TranslationEnd;
    public event EventHandler<GizmoEventArgs>? RotationStart;
    public event EventHandler<GizmoEventArgs>? Rotating;
    public event EventHandler<GizmoEventArgs>? RotationEnd;
    public event EventHandler<GizmoEventArgs>? ScaleStart;
    public event EventHandler<GizmoEventArgs>? Scaling;
    public event EventHandler<GizmoEventArgs>? ScaleEnd;

    ITransformGizmo? _gizmo;
    TransformGizmoHandles _hoveredHandle = TransformGizmoHandles.None;

    public static readonly StyledProperty<bool> DirtyProperty = AvaloniaProperty.Register<TransformGizmoOverlay, bool>(nameof(Dirty), defaultValue: true);
    public bool Dirty
    {
        get => GetValue(DirtyProperty);
        set => SetValue(DirtyProperty, value);
    }

    public static readonly StyledProperty<TransformGizmoTypes> GizmoTypeProperty = AvaloniaProperty.Register<TransformGizmoOverlay, TransformGizmoTypes>(nameof(GizmoType), defaultValue: TransformGizmoTypes.None);
    public TransformGizmoTypes GizmoType
    {
        get => GetValue(GizmoTypeProperty);
        set => SetValue(GizmoTypeProperty, value);
    }

    public TransformGizmoOverlay()
    {
        IsHitTestVisible = true;
    }

    public bool HitTest(Point pt)
    {
        if (_gizmo != null)
            return _gizmo.HitTest(pt);
        return false;
    }

    public void Reset()
    {
        if (_hoveredHandle != TransformGizmoHandles.None)
        {
            _hoveredHandle = TransformGizmoHandles.None;
            _gizmo!.Dragging = false;
            InvalidateVisual();
        }
    }

    public void UpdateRotationVisual(double rotationAngle, Point rayEnd, bool showRay)
    {
        if (_gizmo is RotateGizmo rotateGizmo)
        {
            rotateGizmo.RotationAngle = rotationAngle;
            rotateGizmo.RayEnd = rayEnd;
            rotateGizmo.ShowRay = showRay;
            InvalidateVisual();
        }
    }

    public void UpdateScaleVisual(Vector2 scaleFactor)
    {
        if (_gizmo is ScaleGizmo scaleGizmo)
        {
            scaleGizmo.ScaleFactor = scaleFactor;
            InvalidateVisual();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DirtyProperty)
        {
            if (Dirty)
            {
                InvalidateVisual();
                Dirty = false;
            }
        }
        else if (change.Property == GizmoTypeProperty)
        {
            switch (GizmoType)
            {
                case TransformGizmoTypes.None:
                    _gizmo = null;
                    break;
                case TransformGizmoTypes.Translation:
                    _gizmo = new TranslateGizmo();
                    break;
                case TransformGizmoTypes.Rotation:
                    _gizmo = new RotateGizmo();
                    break;
                case TransformGizmoTypes.Scale:
                    _gizmo = new ScaleGizmo();
                    break;
            }
            InvalidateVisual();
        }
        else if (change.Property == IsVisibleProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!IsVisible) return;
        if (_gizmo != null)
            _gizmo.Render(context, _hoveredHandle);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var point = e.GetPosition(this);
        TransformGizmoHandles hitHandle = _gizmo != null ? _gizmo.HoverTest(point) : TransformGizmoHandles.None;
        if (hitHandle != TransformGizmoHandles.None)
        {
            _hoveredHandle = hitHandle;
            _gizmo!.Dragging = true;
            if (_gizmo is TranslateGizmo)
                TranslationStart?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            else if (_gizmo is RotateGizmo)
                RotationStart?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            else if (_gizmo is ScaleGizmo)
                ScaleStart?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_hoveredHandle != TransformGizmoHandles.None)
        {
            _gizmo!.Dragging = false;
            if (_gizmo is TranslateGizmo)
                TranslationEnd?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            else if (_gizmo is RotateGizmo)
                RotationEnd?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            else if (_gizmo is ScaleGizmo)
                ScaleEnd?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            if (e.Pointer.Captured == this)
                e.Pointer.Capture(null);
            var point = e.GetPosition(this);
            var hitHandle = _gizmo.HoverTest(point);
            if (_hoveredHandle != hitHandle)
            {
                _hoveredHandle = hitHandle;
                InvalidateVisual();
            }
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_gizmo == null) return;
        if (_gizmo.Dragging)
        {
            if (_gizmo is TranslateGizmo)
                Translating?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            else if (_gizmo is RotateGizmo)
                Rotating?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
            else if (_gizmo is ScaleGizmo)
                Scaling?.Invoke(this, new GizmoEventArgs
                {
                    Handle = _hoveredHandle,
                    PointerArgs = e,
                });
        }
        else
        {
            var point = e.GetPosition(this);
            var hitHandle = _gizmo.HoverTest(point);
            if (_hoveredHandle != hitHandle)
            {
                _hoveredHandle = hitHandle;
                InvalidateVisual();
            }
        }
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (_hoveredHandle != TransformGizmoHandles.None)
        {
            _hoveredHandle = TransformGizmoHandles.None;
            Reset();
        }
    }
}
