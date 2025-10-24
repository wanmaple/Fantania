using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class WorldEnvironment : ObservableObject
{
    private double _diffuseModulation = 1.0;
    public double DiffuseModulation
    {
        get { return _diffuseModulation; }
        set
        {
            if (_diffuseModulation != value)
            {
                _diffuseModulation = value;
                OnPropertyChanged(nameof(DiffuseModulation));
            }
        }
    }

    private Vector _directionalLightDir = Vector.Normalize(new Vector(-1.0, 1.0));
    public Vector DirectionalLightDirection
    {
        get { return _directionalLightDir; }
        set
        {
            if (_directionalLightDir != value)
            {
                _directionalLightDir = value;
                OnPropertyChanged(nameof(DirectionalLightDirection));
            }
        }
    }

    private double _directionalLightHeight = 0.0;
    public double DirectionalLightHeight
    {
        get { return _directionalLightHeight; }
        set
        {
            if (_directionalLightHeight != value)
            {
                _directionalLightHeight = value;
                OnPropertyChanged(nameof(DirectionalLightHeight));
            }
        }
    }
}