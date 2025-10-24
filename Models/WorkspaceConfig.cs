using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class WorkspaceConfig : ObservableObject
{
    private int _version;
    public int Version
    {
        get { return _version; }
        set
        {
            if (_version != value)
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
    }

    private bool _modding;
    public bool Modding
    {
        get { return _modding; }
        set
        {
            if (_modding != value)
            {
                _modding = value;
                OnPropertyChanged(nameof(Modding));
            }
        }
    }
}