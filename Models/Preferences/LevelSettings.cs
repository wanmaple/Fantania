using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class LevelSettings : ObservableObject
{
    private int _alignSize = 4;
    public int AlignSize
    {
        get { return _alignSize; }
        set
        {
            if (_alignSize != value)
            {
                _alignSize = value;
                OnPropertyChanged(nameof(AlignSize));
            }
        }
    }
}