using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class DebugSettings : ObservableObject
{
    private bool _bvhVisualizerOn = false;
    public bool IsBVHVisualizerOn
    {
        get { return _bvhVisualizerOn; }
        set
        {
            if (_bvhVisualizerOn != value)
            {
                _bvhVisualizerOn = value;
                OnPropertyChanged(nameof(IsBVHVisualizerOn));
            }
        }
    }

    private Vector4 _bvhVisualMinColor = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
    public Vector4 BVHVisualMinColor
    {
        get { return _bvhVisualMinColor; }
        set
        {
            if (_bvhVisualMinColor != value)
            {
                _bvhVisualMinColor = value;
                OnPropertyChanged(nameof(BVHVisualMinColor));
            }
        }
    }

    private Vector4 _bvhVisualMaxColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 BVHVisualMaxColor
    {
        get { return _bvhVisualMaxColor; }
        set
        {
            if (_bvhVisualMaxColor != value)
            {
                _bvhVisualMaxColor = value;
                OnPropertyChanged(nameof(BVHVisualMaxColor));
            }
        }
    }
}