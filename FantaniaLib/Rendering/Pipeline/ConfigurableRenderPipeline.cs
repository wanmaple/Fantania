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

    public const string COLOR_BUFFER = "Color";

    public IRenderDevice Device => _device;
    public UniformSet GlobalUniforms => _globalUniforms;
    public ShaderCache ShaderCache => _cacheShaders;
    public TextureManager TextureManager => _mgrTextures;
    public MaterialSet MaterialSet => _materials;
    public VertexStreamCache VertexStreamCache => _cacheVertStreams;
    public CommandBuffer CommandBuffer => WorkingBuffer;

    private CommandBuffer CompletedBuffer => _cmdBuffers[_completeBufferIndex];
    private CommandBuffer WorkingBuffer => _cmdBuffers[(_completeBufferIndex + 1) % 2];

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
        // Color is required.
        AddFrameBuffer(new FrameBufferConfig
        {
            Name = COLOR_BUFFER,
            Description = new FrameBufferDescription
            {
                Width = config.Resolution.X,
                Height = config.Resolution.Y,
                ColorFormat = TextureFormats.RGBA8,
                DepthFormat = DepthFormats.Depth24Stencil8,
            },
        });
        foreach (var fbConfig in config.FrameBuffers)
        {
            if (fbConfig.Name == COLOR_BUFFER) continue;
            AddFrameBuffer(fbConfig);
        }
        foreach (var stage in config.Stages)
        {
            AddStage(stage);
        }
        _stageList.StableSort(PipelineStageComparer.Instance);
        foreach (var matInfo in config.Materials)
        {
            string? vertSrc = IOHelper.ReadText(matInfo.VertexShader, workspace);
            string? fragSrc = IOHelper.ReadText(matInfo.FragmentShader, workspace);
            if (vertSrc == null || fragSrc == null) continue;
            var shader = _cacheShaders.Acquire(vertSrc, fragSrc);
            var material = new RenderMaterial
            {
                Shader = shader,
            };
            _materials.AddMaterial(matInfo.MaterialKey, material);
        }
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

    public void StartWorkerThread()
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
            while (!_ctsWorker.IsCancellationRequested)
            {
                try
                {
                    if (!_evTaskStart.WaitOne(0)) continue;
                    var groups = _renderables.GroupBy(r => r.Stage);
                    foreach (var stage in _stageList)
                    {
                        var group = groups.FirstOrDefault(g => g.Key == stage.Name);
                        if (group != null)
                        {
                            stage.PreRender(this);
                            stage.Render(this, group);
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
            foreach (var renderable in renderables)
            {
                _renderables.Add(renderable.Clone());
            }
            _evTaskStart!.Set();
        }
    }

    public void ExecuteCompletedBuffer()
    {
        lock (_mutexBuffers)
            CompletedBuffer.Execute(this);
    }

    public void SyncGlobalUniforms(RenderMaterial material)
    {
        foreach (var pair in _globalUniforms)
        {
            material.Uniforms.SetUniform(pair.Key, pair.Value);
        }
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
}