using FantaniaLib;

namespace Fantania.ViewModels;

public class LevelViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public LevelViewModel(Workspace workspace)
    {
        _workspace = workspace;
    }

    Workspace _workspace;
}