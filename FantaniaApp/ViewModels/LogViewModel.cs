using CommunityToolkit.Mvvm.Input;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class LogViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public LogViewModel(Workspace workspace)
    {
        _workspace = workspace;
    }

    [RelayCommand]
    public void ClearLog()
    {
        _workspace.LogModule.Clear();
    }

    Workspace _workspace;
}