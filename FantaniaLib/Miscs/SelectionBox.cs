using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public sealed class SelectionBox : ObservableObject
{
    public bool IsZero => _orig.X == _cur.X || _orig.Y == _cur.Y;

    private Vector2 _orig = Vector2.Zero;
    public Vector2 Origin
    {
        get { return _orig; }
        set
        {
            if (_orig != value)
            {
                _orig = value;
                OnPropertyChanged(nameof(Origin));
                OnPropertyChanged(nameof(Left));
                OnPropertyChanged(nameof(Right));
                OnPropertyChanged(nameof(Top));
                OnPropertyChanged(nameof(Bottom));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    private Vector2 _cur = Vector2.Zero;
    public Vector2 Current
    {
        get { return _cur; }
        set
        {
            if (_cur != value)
            {
                _cur = value;
                OnPropertyChanged(nameof(Current));
                OnPropertyChanged(nameof(Left));
                OnPropertyChanged(nameof(Right));
                OnPropertyChanged(nameof(Top));
                OnPropertyChanged(nameof(Bottom));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    public void Reset()
    {
        Origin = Vector2.Zero;
        Current = Vector2.Zero;
    }

    public float Left => MathF.Min(_orig.X, _cur.X);
    public float Right => MathF.Max(_orig.X, _cur.X);
    public float Top => MathF.Min(_orig.Y, _cur.Y);
    public float Bottom => MathF.Max(_orig.Y, _cur.Y);
    public float Width => MathF.Abs(_cur.X - _orig.X);
    public float Height => MathF.Abs(_cur.Y - _orig.Y);
}