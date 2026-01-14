using MoonSharp.Interpreter;

namespace FantaniaLib;

[BindingScript(CanInstantiate = false)]
public class WorkspaceProxy
{
    public string RootFolder => _workspace.RootFolder;

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
        _workspace.LogModule.Log(content);
    }

    public void LogOptional(string content)
    {
        _workspace.LogModule.LogOptional(content);
    }

    public void LogWarning(string content)
    {
        _workspace.LogModule.LogWarning(content);
    }

    public void LogError(string content)
    {
        _workspace.LogModule.LogError(content);
    }

    IWorkspace _workspace;
}