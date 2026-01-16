using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public abstract class WorkspaceModule : ObservableObject
{
    protected WorkspaceModule([DisallowNull] IWorkspace workspace)
    {
        _workspace = workspace;
    }

    protected IWorkspace _workspace;
}