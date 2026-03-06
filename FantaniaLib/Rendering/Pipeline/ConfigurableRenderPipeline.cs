using System.Numerics;

namespace FantaniaLib;

public class ConfigurableRenderPipeline : IRenderContext, IDisposable
{
    internal class PipelineStageComparer : IComparer<IPipelineStage>
    {
        public static readonly PipelineStageComparer Instance = new PipelineStageComparer();

        private PipelineStageComparer()
        { }

        public int Compare(IPipelineStage? x, IPipelineStage? y)
        {
            return x!.Order.CompareTo(y!.Order);
        }
    }

    public class RenderStatistics
    {
        public int DrawCalls { get; set; }
        public int Triangles { get; set; }
    }

    private class FrameData
    {
        public int MaxTextureSlot { get; set; }
    }

    public const string COLOR_BUFFER = "Color";
    public const string LIGHT_OCCLUDER_MASK_BUFFER = "LightOccluderMask";
    // 这两张RT用于Jump Flood算法，分别存储当前迭代的结果和上一次迭代的结果，并以其中一张最终做BuildSDF的结果
    public const string JFA1_BUFFER = "JFA1";
    public const string JFA2_BUFFER = "JFA2";

    public IRenderDevice Device => _device;
    public UniformSet GlobalUniforms => _globalUniforms;
    public ShaderCache ShaderCache => _cacheShaders;
    public TextureManager TextureManager => _mgrTextures;
    public MaterialSet MaterialSet => _materials;
    public VertexStreamCache VertexStreamCache => _cacheVertStreams;
    public CommandBuffer CommandBuffer => WorkingBuffer;
    public int LightCullingTileSize { get; private set; }
    public TiledLightCullingData TiledLightCullingData => _tiledLightCullingData;

    public int MaxTextureSlot => _frameData.MaxTextureSlot;

    private CommandBuffer CompletedBuffer => _cmdBuffers[_completeBufferIndex];
    private CommandBuffer WorkingBuffer => _cmdBuffers[(_completeBufferIndex + 1) % 2];

    public RenderStatistics Statistics { get; } = new RenderStatistics();

    public ConfigurableRenderPipeline(IRenderDevice device)
    {
        _device = device;
        _cacheShaders = new ShaderCache(device);
        _mgrTextures = new TextureManager(device);
        _materials = new MaterialSet(_cacheShaders.Fallback);
        _cacheVertStreams = new VertexStreamCache(device);
    }

    public FrameBuffer? GetFrameBuffer(string name)
    {
        _fbs.TryGetValue(name, out var fb);
        return fb;
    }

    public void Build(RenderPipelineConfig config, IWorkspace workspace)
    {
        if (_built) return;
        foreach (var fbConfig in config.FrameBuffers)
        {
            AddFrameBuffer(fbConfig);
        }
        foreach (var stage in config.Stages)
        {
            AddStage(stage);
        }
        _stageList.StableSort(PipelineStageComparer.Instance);
        foreach (var (key, vertPath, fragPath) in BUILTIN_SHADER_SOURCES)
        {
            string vertSrc = IOHelper.ReadText(vertPath, workspace)!;
            string fragSrc = IOHelper.ReadText(fragPath, workspace)!;
            ShaderProgram shader = _cacheShaders.Acquire(vertSrc, fragSrc);
            _materials.AddShader(key, shader);
        }
        foreach (var matInfo in config.Materials)
        {
            string? vertSrc = IOHelper.ReadText(matInfo.VertexShader, workspace);
            string? fragSrc = IOHelper.ReadText(matInfo.FragmentShader, workspace);
            if (vertSrc == null || fragSrc == null) continue;
            ShaderProgram shader = _cacheShaders.Acquire(vertSrc, fragSrc);
            _materials.AddShader(matInfo.MaterialKey, shader);
        }
        LightCullingTileSize = config.LightCullingTileSize;
        _built = true;
    }

    void AddFrameBuffer(FrameBufferConfig config)
    {
        if (_built) return;
        if (_fbs.TryGetValue(config.Name, out FrameBuffer? fb))
            fb.Dispose(_device);
        _fbs[config.Name] = _device.CreateFrameBuffer(config.Description);
    }

    void AddStage(IPipelineStage stage)
    {
        if (_built) return;
        if (!_stageMap.ContainsKey(stage.Name))
        {
            _stageList.Add(stage);
            _stageMap[stage.Name] = stage;
        }
    }

    public void StartWorkerThread(IWorkspace workspace, Camera2D camera)
    {
        _ctsWorker = new CancellationTokenSource();
        _evTaskStart = new AutoResetEvent(false);
        _evTaskComplete = new AutoResetEvent(true);
        for (int i = 0; i < _cmdBuffers.Length; i++)
        {
            _cmdBuffers[i] = new CommandBuffer();
        }
        _worker = new Thread(() =>
        {
            WaitHandle[] waitHandles = new[] { _ctsWorker.Token.WaitHandle, _evTaskStart };
            while (true)
            {
                try
                {
                    int signaled = WaitHandle.WaitAny(waitHandles);
                    if (signaled == 0)
                        break;
                    var pipelineHook = workspace.ScriptingModule.GetPipelineHookOrDefault();
                    foreach (var uniform in pipelineHook.Uniforms)
                    {
                        string uniformName = uniform.Name;
                        if (!string.IsNullOrEmpty(uniformName))
                        {
                            if (uniform.Type >= PipelineHookUniformTypes.FrameBufferColorAttachment0 && uniform.Type < PipelineHookUniformTypes.FrameBufferDepthAttachment)
                            {
                                string fbName = (string)uniform.Value;
                                int index = (int)(uniform.Type - PipelineHookUniformTypes.FrameBufferColorAttachment0);
                                FrameBuffer? fb = GetFrameBuffer(fbName);
                                if (fb == null)
                                {
                                    workspace.LogWarning($"Pipeline Hook Warning: FrameBuffer '{fbName}' not found for uniform '{uniformName}'.");
                                    continue;
                                }
                                if (index >= fb.ColorAttachments.Count)
                                {
                                    workspace.LogWarning($"Pipeline Hook Warning: FrameBuffer '{fbName}' does not have color attachment at index {index} for uniform '{uniformName}'.");
                                    continue;
                                }
                                GlobalUniforms.SetUniform(uniformName, TextureDefinition.CreateGpuDefinition(fb.ColorAttachmentAt(index)), ++_frameData.MaxTextureSlot);
                            }
                            else if (uniform.Type == PipelineHookUniformTypes.FrameBufferDepthAttachment)
                            {
                                string fbName = (string)uniform.Value;
                                FrameBuffer? fb = GetFrameBuffer(fbName);
                                if (fb == null)
                                {
                                    workspace.LogWarning($"Pipeline Hook Warning: FrameBuffer '{fbName}' not found for uniform '{uniformName}'.");
                                    continue;
                                }
                                if (fb.Description.DepthFormat == DepthFormats.None)
                                {
                                    workspace.LogWarning($"Pipeline Hook Warning: FrameBuffer '{fbName}' does not have a depth attachment for uniform '{uniformName}'.");
                                    continue;
                                }
                                GlobalUniforms.SetUniform(uniformName, TextureDefinition.CreateGpuDefinition(fb.DepthAttachment), ++_frameData.MaxTextureSlot);
                            }
                            else
                            {
                                switch (uniform.Type)
                                {
                                    case PipelineHookUniformTypes.Int1:
                                        GlobalUniforms.SetUniform(uniformName, (int)uniform.Value);
                                        break;
                                    case PipelineHookUniformTypes.Float1:
                                        GlobalUniforms.SetUniform(uniformName, (float)uniform.Value);
                                        break;
                                    case PipelineHookUniformTypes.Float2:
                                        GlobalUniforms.SetUniform(uniformName, (Vector2)uniform.Value);
                                        break;
                                    case PipelineHookUniformTypes.Float3:
                                        GlobalUniforms.SetUniform(uniformName, (Vector3)uniform.Value);
                                        break;
                                    case PipelineHookUniformTypes.Float4:
                                        GlobalUniforms.SetUniform(uniformName, (Vector4)uniform.Value);
                                        break;
                                    case PipelineHookUniformTypes.Int1Array:
                                        GlobalUniforms.SetUniform(uniformName, ((IReadOnlyList<int>)uniform.Value).ToArray());
                                        break;
                                    case PipelineHookUniformTypes.Float1Array:
                                        GlobalUniforms.SetUniform(uniformName, ((IReadOnlyList<float>)uniform.Value).ToArray());
                                        break;
                                    case PipelineHookUniformTypes.Float2Array:
                                        GlobalUniforms.SetUniform(uniformName, ((IReadOnlyList<Vector2>)uniform.Value).ToArray());
                                        break;
                                    case PipelineHookUniformTypes.Float3Array:
                                        GlobalUniforms.SetUniform(uniformName, ((IReadOnlyList<Vector3>)uniform.Value).ToArray());
                                        break;
                                    case PipelineHookUniformTypes.Float4Array:
                                        GlobalUniforms.SetUniform(uniformName, ((IReadOnlyList<Vector4>)uniform.Value).ToArray());
                                        break;
                                }
                            }
                        }
                    }
                    var groups = _renderables.GroupBy(r => r.Stage);
                    foreach (var stage in _stageList)
                    {
                        stage.Setup(this);
                    }
                    foreach (var stage in _stageList)
                    {
                        var group = groups.FirstOrDefault(g => g.Key == stage.Name);
                        if (group != null)
                        {
                            stage.PreRender(this);
                            stage.Render(this, group, camera);
                            stage.PostRender(this);
                        }
                    }
                    SwapCommandBuffer();
                    _evTaskComplete.Set();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Pipeline Worker Thread Exception: {ex}");
                    _evTaskComplete.Set();
                }
            }
            _evTaskStart?.Dispose();
            _evTaskComplete?.Dispose();
        })
        {
            Name = "Pipeline Worker Thread",
            IsBackground = true,
        };
        _worker.Start();
    }

    void SwapCommandBuffer()
    {
        lock (_mutexBuffers)
        {
            _completeBufferIndex = (_completeBufferIndex + 1) % 2;
            WorkingBuffer.Clear();
        }
    }

    public void ReceiveRenderables(IEnumerable<IRenderable> renderables)
    {
        if (_evTaskComplete!.WaitOne(-1))
        {
            _renderables.Clear();
            _renderables.AddRange(renderables);
            int maxTextureSlot = 0;
            foreach (var r in _renderables)
            {
                var set = r.Material.Uniforms;
                foreach (var name in set.Names)
                {
                    var uniform = set[name];
                    if (uniform.Type == UniformTypes.Texture)
                    {
                        var texInfo = uniform.Get<UniformSet.TextureInformation>();
                        int slot = texInfo.TextureSlot;
                        if (slot > maxTextureSlot)
                            maxTextureSlot = slot;
                    }
                    else if (uniform.Type == UniformTypes.TextureArray)
                    {
                        var texArrayInfo = uniform.Get<UniformSet.TextureArrayInformation>();
                        foreach (int slot in texArrayInfo.TextureSlots)
                        {
                            if (slot > maxTextureSlot)
                                maxTextureSlot = slot;
                        }
                    }
                }
            }
            _frameData.MaxTextureSlot = maxTextureSlot;
            _evTaskStart!.Set();
        }
    }

    public void ExecuteCompletedBuffer()
    {
        lock (_mutexBuffers)
            CompletedBuffer.Execute(this);
    }

    public void Tick()
    {
        _mgrTextures.Tick();
    }

    public void ResetStatistics()
    {
        Statistics.DrawCalls = 0;
        Statistics.Triangles = 0;
    }

    public void Dispose()
    {
        _cacheShaders.Dispose();
        _mgrTextures.Dispose();
        foreach (var fb in _fbs.Values)
        {
            fb.Dispose(_device);
        }
        _ctsWorker?.Cancel();
        _evTaskStart?.Set();
        _worker?.Join(1000);
    }

    IRenderDevice _device;
    Dictionary<string, FrameBuffer> _fbs = new Dictionary<string, FrameBuffer>(8);
    UniformSet _globalUniforms = new UniformSet();

    List<IPipelineStage> _stageList = new List<IPipelineStage>(8);
    Dictionary<string, IPipelineStage> _stageMap = new Dictionary<string, IPipelineStage>(8);
    bool _built = false;

    ShaderCache _cacheShaders;
    TextureManager _mgrTextures;
    MaterialSet _materials;
    VertexStreamCache _cacheVertStreams;

    CancellationTokenSource? _ctsWorker;
    Thread? _worker;
    AutoResetEvent? _evTaskStart;
    AutoResetEvent? _evTaskComplete;
    List<IRenderable> _renderables = new List<IRenderable>(0);
    object _mutexBuffers = new object();
    CommandBuffer[] _cmdBuffers = new CommandBuffer[2];
    volatile int _completeBufferIndex;
    FrameData _frameData = new FrameData();
    TiledLightCullingData _tiledLightCullingData = new TiledLightCullingData();

    readonly (string, string, string)[] BUILTIN_SHADER_SOURCES = new[]
    {
        ("#FantaniaFallback", "avares://Fantania/Assets/shaders/vert_standard.vs", "avares://Fantania/Assets/shaders/frag_fallback.fs"),
        ("#FantaniaStandard", "avares://Fantania/Assets/shaders/vert_standard.vs", "avares://Fantania/Assets/shaders/frag_standard.fs"),
        ("#FantaniaSDFSeed", "avares://Fantania/Assets/shaders/vert_fullscreen.vs", "avares://Fantania/Assets/shaders/frag_sdf_seed.fs"),
        ("#FantaniaSDFJump", "avares://Fantania/Assets/shaders/vert_fullscreen.vs", "avares://Fantania/Assets/shaders/frag_sdf_jump.fs"),
        ("#FantaniaSDFBuild", "avares://Fantania/Assets/shaders/vert_fullscreen.vs", "avares://Fantania/Assets/shaders/frag_sdf_build.fs"),
    };
}