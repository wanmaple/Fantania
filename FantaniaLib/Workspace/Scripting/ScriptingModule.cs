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
                    _workspace.LogError($"Invalid entity script: {scriptPath}");
                    _workspace.LogError($"Detail: {ex}");
                }
            }
        }
        string configFolder = _workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER, Workspace.CONFIGS_FOLDER);
        if (Directory.Exists(configFolder))
        {
            var di = new DirectoryInfo(configFolder);
            foreach (var fi in di.GetFiles("*.lua", SearchOption.AllDirectories))
            {
                string scriptPath = fi.FullName.ToStandardPath();
                try
                {
                    string script = File.ReadAllText(scriptPath);
                    DynValue table = _scriptEngine.ExecuteString(script);
                    if (table.IsNil())
                    {
                        _workspace.LogError($"GameData '{scriptPath}' returns nil.");
                        continue;
                    }
                    var template = new GameDataTemplate(_scriptEngine, table);
                    if (string.IsNullOrEmpty(template.DataGroup))
                    {
                        _workspace.LogWarning($"GameData '{scriptPath}' has empty data group, skipped.");
                        continue;
                    }
                    _workspace.DatabaseModule.AddGameDataTemplate(template);
                }
                catch (Exception ex)
                {
                    _workspace.LogError($"Invalid config script: {scriptPath}");
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
        if (_renderPipelineCfg == null)
        {
            string pipelineSetupScriptPath = _workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER, "pipeline_setup.lua");
            if (File.Exists(pipelineSetupScriptPath))
            {
                DynValue config = _scriptEngine.ExecuteFile(pipelineSetupScriptPath);
                if (config != null && config.Type == DataType.Table)
                {
                    try
                    {
                        _renderPipelineCfg = config.ToObject<RenderPipelineConfig>();
                    }
                    catch (Exception ex)
                    {
                        _workspace.LogError("pipeline_setup.lua is corrupt.");
                        _workspace.LogError($"Detail: {ex}");
                        _renderPipelineCfg = null;
                    }
                }
            }
            if (_renderPipelineCfg == null)
            {
                _renderPipelineCfg = new RenderPipelineConfig
                {
                    Resolution = new Vector2Int(1920, 1080),
                    DefaultTextureFilter = TextureFilters.LinearClamp,
                    LightCullingTileSize = 32,
                    FrameBuffers = [
                        new FrameBufferConfig
                        {
                            Name = ConfigurableRenderPipeline.COLOR_BUFFER,
                            Description = new FrameBufferDescription
                            {
                                Width = 1920,
                                Height = 1080,
                                ColorDescription = new FrameBufferColorDescription
                                {
                                    Format = TextureFormats.RGBA8,
                                    MinFilter = TextureMinFilters.Linear,
                                    MagFilter = TextureMagFilters.Linear,
                                    WrapS = TextureWraps.ClampToEdge,
                                    WrapT = TextureWraps.ClampToEdge,
                                },
                                DepthFormat = DepthFormats.Depth24Stencil8,
                            },
                        },
                    ],
                    Stages = [
                        BuiltinPipelineStages.Opaque,
                        BuiltinPipelineStages.Transparent,
                    ],
                    Materials = [],
                };
            }
        }
        return _renderPipelineCfg;
    }

    public PipelineHook GetPipelineHookOrDefault()
    {
        if (_pipelineHook == null)
        {
            string pipelineSetupScriptPath = _workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER, "pipeline_hook.lua");
            if (File.Exists(pipelineSetupScriptPath))
            {
                DynValue hook = _scriptEngine.ExecuteFile(pipelineSetupScriptPath);
                if (hook != null && hook.Type == DataType.Table)
                {
                    try
                    {
                        _pipelineHook = hook.ToObject<PipelineHook>();
                    }
                    catch (Exception ex)
                    {
                        _workspace.LogError("pipeline_hook.lua is corrupt.");
                        _workspace.LogError($"Detail: {ex}");
                        _pipelineHook = null;
                    }
                }
            }
            if (_pipelineHook == null)
            {
                _pipelineHook = new PipelineHook();
            }
        }
        return _pipelineHook;
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

    public ExportSettings? GetExportSettings()
    {
        if (_exportSettings == null)
        {
            string scriptPath = _workspace.GetAbsolutePath(Workspace.SCRIPTS_FOLDER, "export_settings.lua");
            if (File.Exists(scriptPath))
            {
                DynValue settings = _scriptEngine.ExecuteFile(scriptPath);
                if (settings != null && settings.Type == DataType.Table)
                {
                    try
                    {
                        var template = new ScriptTemplate(_scriptEngine, settings);
                        _exportSettings = new ExportSettings(template);
                    }
                    catch (Exception ex)
                    {
                        _workspace.LogError("export_settings.lua is corrupt.");
                        _workspace.LogError($"Detail: {ex}");
                        _exportSettings = null;
                    }
                }
            }
        }
        return _exportSettings;
    }

    RenderPipelineConfig? _renderPipelineCfg;
    PipelineHook? _pipelineHook;
    ExportSettings? _exportSettings;

    ScriptEngine _scriptEngine = new ScriptEngine();
}