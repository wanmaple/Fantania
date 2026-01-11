using System.Diagnostics.CodeAnalysis;

namespace FantaniaLib;

public abstract class WorkspaceModule
{
    protected WorkspaceModule([DisallowNull] IWorkspace workspace)
    {
        _workspace = workspace;
    }

    protected IWorkspace _workspace;
}