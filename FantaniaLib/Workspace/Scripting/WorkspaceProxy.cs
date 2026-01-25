using MoonSharp.Interpreter;

namespace FantaniaLib;

[BindingScript(CanInstantiate = false)]
public class WorkspaceProxy
{
    public string RootFolder => _workspace.RootFolder;

    internal IWorkspace RealWorkspace => _workspace;

    public WorkspaceProxy(IWorkspace workspace)
    {
        _workspace = workspace;
    }

    public string GetAbsolutePath(params string[] pathes)
    {
        return _workspace.GetAbsolutePath(pathes);
    }

    public DynValue CreateScriptableInstance()
    {
        return _workspace.ScriptingModule.CreateScriptableInstance();
    }

    public void Log(string content)
    {
        _workspace.Log(content);
    }

    public void LogOptional(string content)
    {
        _workspace.LogOptional(content);
    }

    public void LogWarning(string content)
    {
        _workspace.LogWarning(content);
    }

    public void LogError(string content)
    {
        _workspace.LogError(content);
    }

    IWorkspace _workspace;
}