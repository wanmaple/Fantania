using System.Diagnostics.CodeAnalysis;
using FantaniaLib;

namespace Fantania.ViewModels;

public class WorkspaceViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public WorkspaceViewModel([DisallowNull] Workspace workspace)
    {
        _workspace = workspace;
    }

    Workspace _workspace;
}