using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public abstract class WorkspaceModule : ObservableObject
{
    protected WorkspaceModule(IWorkspace workspace)
    {
        _workspace = workspace;
    }

    protected IWorkspace _workspace;
}