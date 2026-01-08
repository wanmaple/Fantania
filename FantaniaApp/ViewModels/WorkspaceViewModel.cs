using System.Diagnostics.CodeAnalysis;
using FantaniaLib;

namespace Fantania.ViewModels;

public class WorkspaceViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public PlacementViewModel PlacementViewModel => _vmPlacements;
    public LogViewModel LogViewModel => _vmLogs;

    public WorkspaceViewModel([DisallowNull] Workspace workspace)
    {
        _workspace = workspace;
        _vmPlacements = new PlacementViewModel(workspace);
        _vmLogs = new LogViewModel(_workspace);
    }

    Workspace _workspace;
    PlacementViewModel _vmPlacements;
    LogViewModel _vmLogs;
}