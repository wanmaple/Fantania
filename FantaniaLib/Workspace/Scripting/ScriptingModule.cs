using MoonSharp.Interpreter;

namespace FantaniaLib;

public class ScriptingModule : WorkspaceModule
{
    private class WorkspaceScriptLoader : FantaniaScriptLoader
    {
        public WorkspaceScriptLoader(IWorkspace workspace)
        {
            _workspace = workspace;
            ModulePaths = ModulePaths.Concat([$"{_workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER)}/?.lua",]).ToArray();
        }

        public override object LoadFile(string file, Table globalContext)
        {
            if (file.StartsWith("avares://"))
            {
                return base.LoadFile(file, globalContext);
            }
            return File.ReadAllText(file);
        }

        public override bool ScriptFileExists(string name)
        {
            if (!base.ScriptFileExists(name))
            {
                return File.Exists(name);
            }
            return true;
        }

        IWorkspace _workspace;
    }

    public ScriptEngine ScriptEngine => _scriptEngine;

    public ScriptingModule(IWorkspace workspace) : base(workspace)
    {
        _scriptEngine.SetCustomScriptLoader(new WorkspaceScriptLoader(workspace));
    }

    public void LoadBuiltinScripts()
    {
        foreach (string scriptPath in AvaloniaHelper.EnumerateAssetFolder("avares://Fantania/Assets/scripts/templates"))
        {
            if (scriptPath.EndsWith(".lua"))
            {
                string script = AvaloniaHelper.ReadAssetText(scriptPath);
                var template = new PlacementTemplate(_scriptEngine, _scriptEngine.ExecuteString(script));
                _workspace.PlacementModule.AddLevelTemplate(template);
            }
        }
    }

    public DynValue CreateScriptableInstance()
    {
        var table = DynValue.NewTable(_scriptEngine.Environment);
        _scriptEngine.SetInstanceMember(table, "__env", _scriptEngine);
        return table;
    }

    public RenderPipelineConfig GetCustomRenderPipelineConfigOrDefault()
    {
        RenderPipelineConfig? ret = null;
        string pipelineSetupScriptPath = _workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER, "pipeline_setup.lua");
        if (File.Exists(pipelineSetupScriptPath))
        {
            DynValue config = _scriptEngine.ExecuteFile(pipelineSetupScriptPath);
            if (config != null && config.Type == DataType.Table)
            {
                ret = config.ToObject<RenderPipelineConfig>();
            }
        }
        if (ret == null)
        {
            ret = new RenderPipelineConfig
            {
                Resolution = new Vector2Int(1920, 1080),
                FrameBuffers = [],
                Stages = [
                    BuiltinPipelineStages.Opaque,
                    BuiltinPipelineStages.Transparent,
                ],
            };
        }
        return ret;
    }
    
    ScriptEngine _scriptEngine = new ScriptEngine();
}