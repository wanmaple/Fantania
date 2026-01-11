namespace FantaniaLib;

[BindingScript]
public class WorkspaceProxy
{
    public string RootFolder => _workspace.RootFolder;

    public WorkspaceProxy(IWorkspace workspace)
    {
        _workspace = workspace;
    }

    IWorkspace _workspace;
}