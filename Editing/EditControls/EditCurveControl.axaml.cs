using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace Fantania;

public partial class EditCurveControl : UserControl, ICustomHitTest
{
    public const double PADDING = 12.0;
    public const double PADDING_2 = PADDING * 2.0;

    public IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<EditCurveControl, int>(nameof(SelectedIndex), defaultValue: -1);
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<HermitPivot>> PivotsProperty = AvaloniaProperty.Register<EditCurveControl, ObservableCollection<HermitPivot>>(nameof(Pivots), defaultValue: null);
    public ObservableCollection<HermitPivot> Pivots
    {
        get => GetValue(PivotsProperty);
        set => SetValue(PivotsProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<Point>> PointsProperty = AvaloniaProperty.Register<EditCurveControl, ObservableCollection<Point>>(nameof(Points), defaultValue: null);
    public ObservableCollection<Point> Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public EditCurveControl()
    {
        InitializeComponent();
        Points = new ObservableCollection<Point>();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        OnCurveChanged(EditableProperty.EditValue);
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
        EditableProperty.ValueChanged += OnCurveChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        EditableProperty.ValueChanged -= OnCurveChanged;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed)
        {
            Point pos = e.GetPosition(canvas);
            pos += new Point(-PADDING, -PADDING);
            int index = GetIndexFromPositionX(pos.X);
            double height = canvas.Height - PADDING_2;
            if (pos.Y >= 0.0 && pos.Y <= height)
            {
                if (index >= 0 && index < HermitCurve.SEGMENTS)
                {
                    float value = 1.0f - (float)(pos.Y / height);
                    if (_curve.TryGetPivot(index, out var pivot))
                    {
                        _curve.UpdatePivot(SelectedIndex, value, pivot.Tangent);
                    }
                    else
                    {
                        _curve.AddPivot(index, value, 0.0f);
                    }
                    Pivots = new ObservableCollection<HermitPivot>(_curve.Pivots);
                    RegenerateCurvePoints();
                    SelectedIndex = index;
                    _dragging = true;
                }
            }
        }
        else if (e.Properties.IsMiddleButtonPressed)
        {
            Point pos = e.GetPosition(canvas);
            pos += new Point(-PADDING, -PADDING);
            int index = GetIndexFromPositionX(pos.X);
            double height = canvas.Height - PADDING_2;
            if (pos.Y >= 0.0 && pos.Y <= height)
            {
                // 头尾不能删
                if (index > 0 && index < HermitCurve.SEGMENTS - 1)
                {
                    if (_curve.TryGetPivot(index, out var pivot))
                    {
                        _curve.RemovePivot(index);
                        Pivots = new ObservableCollection<HermitPivot>(_curve.Pivots);
                        RegenerateCurvePoints();
                        SelectedIndex = -1;
                    }
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _dragging = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_dragging)
        {
            Point pos = e.GetPosition(canvas);
            pos += new Point(-PADDING, -PADDING);
            int index = GetIndexFromPositionX(pos.X);
            if (pos.Y > 0.0 && pos.Y < canvas.Height - PADDING_2)
            {
                if (index >= 0 && index < HermitCurve.SEGMENTS)
                {
                    SelectedIndex = index;
                }
                else
                {
                    SelectedIndex = -1;
                }
            }
            else
            {
                SelectedIndex = -1;
            }
        }
        else
        {
            Point pos = e.GetPosition(canvas);
            pos += new Point(-PADDING, -PADDING);
            double height = canvas.Height - PADDING_2;
            double y = MathHelper.Clamp(pos.Y, 0.0, height);
            float value = 1.0f - (float)(y / height);
            if (_curve.TryGetPivot(SelectedIndex, out var pivot))
            {
                _curve.UpdatePivot(SelectedIndex, value, pivot.Tangent);
                Pivots = new ObservableCollection<HermitPivot>(_curve.Pivots);
                RegenerateCurvePoints();
            }
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (SelectedIndex >= 0)
        {
            if (_curve.TryGetPivot(SelectedIndex, out var pivot))
            {
                const float SPEED = 0.2f;
                double delta = e.Delta.Y;
                pivot.Tangent = MathHelper.Clamp(pivot.Tangent + (float)delta * SPEED, HermitPivot.MIN_TANGENT, HermitPivot.MAX_TANGENT);
                _curve.UpdatePivot(SelectedIndex, pivot.Value, pivot.Tangent);
                Pivots = new ObservableCollection<HermitPivot>(_curve.Pivots);
                RegenerateCurvePoints();
            }
        }
        e.Handled = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        // SelectedIndex = -1;
        // _dragging = false;
    }

    int GetIndexFromPositionX(double x)
    {
        double width = canvas.Width - PADDING_2;
        double segSize = width / (HermitCurve.SEGMENTS - 1);
        double seg = Math.Floor(x / segSize) + 0.5;
        int index = MathHelper.FloorToInt(seg);
        return index;
    }

    void RegenerateCurvePoints()
    {
        Points.Clear();
        const int PRECISION = 120;
        double width = canvas.Bounds.Width - PADDING_2;
        double height = canvas.Bounds.Height - PADDING_2;
        for (int i = 0; i <= PRECISION; i++)
        {
            double x = i * width / PRECISION + PADDING;
            double y = (1.0 - _curve.Evaluate((float)i / PRECISION)) * height + PADDING;
            Points.Add(new Point(x, y));
        }
    }

    void OnCurveChanged(object curve)
    {
        if (EditableProperty == null) return;
        _curve = ((HermitCurve)EditableProperty.EditValue).Clone();
        Pivots = new ObservableCollection<HermitPivot>(_curve.Pivots);
        RegenerateCurvePoints();
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (EditableProperty != null && !EditableProperty.EditValue.Equals(_curve))
            {
                EditableProperty.EditValue = _curve.Clone();
            }
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    public bool HitTest(Point point)
    {
        return point.X > 0.0 && point.X < Bounds.Width && point.Y > 0.0 && point.Y < Bounds.Height;
    }

    HermitCurve _curve;
    bool _dragging = false;
}