using System.Numerics;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class TiledSnapPoint : ObservableObject, ISnapPoint
{
    public Vector2 Position { get; set; }
    public SnapPointShapes Shape => SnapPointShapes.Circle;
    public Color Color => Colors.PaleVioletRed;
    
    private float _size = 8.0f;
    public float Size
    {
        get { return _size; }
        set
        {
            if (_size != value)
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }
    }

    private bool _active = false;
    public bool IsActive
    {
        get { return _active; }
        set
        {
            if (_active != value)
            {
                _active = value;
                OnPropertyChanged(nameof(IsActive));
                Size = _active ? 12.0f : 8.0f;
            }
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is TiledSnapPoint point && Position.Equals(point.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}