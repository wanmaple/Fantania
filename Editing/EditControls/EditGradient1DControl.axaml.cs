using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class GradientPivot : ObservableObject
{
    private int _index;
    public int Index
    {
        get { return _index; }
        set
        {
            if (_index != value)
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }
    }

    private Vector4 _color;
    public Vector4 Color
    {
        get { return _color; }
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
    }
}

public partial class EditGradient1DControl : UserControl, ICustomHitTest
{
    public IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<EditGradient1DControl, int>(nameof(SelectedIndex), defaultValue: -1);
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<GradientPivot>> PivotsProperty = AvaloniaProperty.Register<EditGradient1DControl, ObservableCollection<GradientPivot>>(nameof(Pivots), defaultValue: null);
    public ObservableCollection<GradientPivot> Pivots
    {
        get => GetValue(PivotsProperty);
        set => SetValue(PivotsProperty, value);
    }

    public EditGradient1DControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (EditableProperty == null) return;
        OnGradientChanged(EditableProperty.EditValue);
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
        EditableProperty.ValueChanged += OnGradientChanged;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Point pos = e.GetPosition(bg);
        if (e.Properties.IsMiddleButtonPressed)
        {
            if (SelectedIndex >= 0 && SelectedIndex != 0 && SelectedIndex != Gradient1D.SEGMENTS - 1)
            {
                _gradient.EraseColor(SelectedIndex);
                SelectedIndex = -1;
            }
        }
        else if (e.Properties.IsLeftButtonPressed)
        {
            if (pos.X > 0.0 && pos.X < bg.Bounds.Width && pos.Y > 0.0 && pos.Y < bg.Bounds.Height)
            {
                int idx = GetIndexFromPositionX(pos.X);
                if (!Pivots.Any(pivot => pivot.Index == idx))
                {
                    Vector4 color = colorpicker.Color.ToVector4().Srgb2Linear();
                    _gradient.InsertColor(idx, color);
                }
                SelectedIndex = idx;
                colorpicker.Color = _gradient.ColorAt(SelectedIndex).Linear2Srgb().ToColor();
            }
            else
            {
                SelectedIndex = -1;
            }
        }
    }

    void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        if (SelectedIndex >= 0)
        {
            _gradient.UpdateColor(SelectedIndex, e.NewColor.ToVector4().Srgb2Linear());
        }
    }

    public bool HitTest(Point point)
    {
        return true;
    }

    int GetIndexFromPositionX(double x)
    {
        double width = bg.Bounds.Width;
        double segSize = width / (Gradient1D.SEGMENTS - 1);
        if (x < segSize * 0.5)
            return 0;
        else if (x > width - segSize * 0.5)
            return Gradient1D.SEGMENTS - 1;
        double seg = Math.Floor(x / segSize + 0.5);
        int index = MathHelper.FloorToInt(seg);
        return index;
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (EditableProperty != null && !EditableProperty.EditValue.Equals(_gradient))
            {
                EditableProperty.EditValue = _gradient.Clone();
            }
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    void OnGradientChanged(object gradient)
    {
        if (EditableProperty == null) return;
        _gradient = ((Gradient1D)EditableProperty.EditValue).Clone();
        Pivots = new ObservableCollection<GradientPivot>(_gradient.Stops.Select(tuple => new GradientPivot
        {
            Index = tuple.Item1,
            Color = tuple.Item2.Linear2Srgb(),
        }));
    }

    Gradient1D _gradient;
}