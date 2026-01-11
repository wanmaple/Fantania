using FantaniaLib;

namespace Fantania.ViewModels;

public class PlacementViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;
    
    private UserPlacement? _selected = null;
    public UserPlacement? SelectedPlacement
    {
        get { return _selected; }
        set
        {
            if (_selected != value)
            {
                _selected = value;
                OnPropertyChanged(nameof(SelectedPlacement));
            }
        }
    }

    public PlacementViewModel(Workspace workspace)
    {
        _workspace = workspace;
    }

    Workspace _workspace;
}