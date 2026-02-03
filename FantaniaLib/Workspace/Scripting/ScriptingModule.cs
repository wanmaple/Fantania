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
        string fallbackScript = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/scripts/placement_fallback.lua");
        DynValue fallbackTable = _scriptEngine.ExecuteString(fallbackScript);
        _workspace.PlacementModule.FallbackTemplate = new PlacementTemplate(_scriptEngine, fallbackTable);
        string entityFolder = _workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER, Workspace.ENTITIES_FOLDER);
        if (Directory.Exists(entityFolder))
        {
            var di = new DirectoryInfo(entityFolder);
            foreach (var fi in di.GetFiles("*.lua", SearchOption.AllDirectories))
            {
                string scriptPath = fi.FullName.ToStandardPath();
                try
                {
                    string script = File.ReadAllText(scriptPath);
                    DynValue table = _scriptEngine.ExecuteString(script);
                    if (table.IsNil())
                    {
                        _workspace.LogError($"Entity '{scriptPath}' returns nil.");
                        continue;
                    }
                    var template = new PlacementTemplate(_scriptEngine, table);
                    _workspace.PlacementModule.AddLevelTemplate(template);
                }
                catch (Exception ex)
                {
                    _workspace.LogError($"Invalid entity: {scriptPath}");
                    _workspace.LogError($"Detail: {ex}");
                }
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
                try
                {
                    ret = config.ToObject<RenderPipelineConfig>();
                }
                catch (Exception ex)
                {
                    _workspace.LogError("pipeline_setup.lua is corrupt.");
                    _workspace.LogError($"Detail: {ex}");
                    ret = null;
                }
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
                    BuiltinPipelineStages.PostProcessing,
                ],
                Materials = [],
            };
        }
        return ret;
    }

    public LevelEditConfig GetCustomLevelEditConfigOrDefault()
    {
        LevelEditConfig? ret = null;
        string editSetupScriptPath = _workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER, "editor_setup.lua");
        if (File.Exists(editSetupScriptPath))
        {
            DynValue config = _scriptEngine.ExecuteFile(editSetupScriptPath);
            if (config != null && config.Type == DataType.Table)
            {
                try
                {
                    ret = config.ToObject<LevelEditConfig>();
                }
                catch (Exception ex)
                {
                    _workspace.LogError("editor_setup.lua is corrupt.");
                    _workspace.LogError($"Detail: {ex}");
                    ret = null;
                }
            }
        }
        if (ret == null)
            ret = new LevelEditConfig();
        return ret;
    }

    ScriptEngine _scriptEngine = new ScriptEngine();
}