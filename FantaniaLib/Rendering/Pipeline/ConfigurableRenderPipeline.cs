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

    public const string COLOR_BUFFER = "Color";

    public IRenderDevice Device => _device;
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
        _materials = new MaterialSet();
        _cacheVertStreams = new VertexStreamCache(device);
    }

    public FrameBuffer? GetFrameBuffer(string name)
    {
        _fbs.TryGetValue(name, out var fb);
        return fb;
    }

    public void SetGlobalUniform(string name, float value)
    {
        if (!_globalUniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float1, value);
            _globalUniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
        }
    }

    public void SetGlobalUniform(string name, Vector2 value)
    {
        if (!_globalUniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float2, value);
            _globalUniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
        }
    }

    public void SetGlobalUniform(string name, Vector3 value)
    {
        if (!_globalUniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float3, value);
            _globalUniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
        }
    }

    public void SetGlobalUniform(string name, Vector4 value)
    {
        if (!_globalUniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Float4, value);
            _globalUniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
        }
    }

    public void SetGlobalUniform(string name, Matrix3x3 value)
    {
        if (!_globalUniforms.TryGetValue(name, out var val))
        {
            val = new MaterialUniform(MaterialUniform.UniformType.Matrix3x3, value);
            _globalUniforms.Add(name, val);
        }
        else
        {
            val.Set(value);
        }
    }

    public void Build(RenderPipelineConfig config, IWorkspace workspace)
    {
        if (_built) return;
        // Color is the required one.
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
        _evTaskStart = new ManualResetEventSlim(false);
        _evTaskComplete = new ManualResetEventSlim(true);
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
                    if (!_evTaskStart.Wait(0)) continue;
                    _evTaskStart.Reset();
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

    public void CollectRenderables(IEnumerable<IRenderable> renderables)
    {
        if (_evTaskComplete!.Wait(-1))
        {
            _evTaskComplete.Reset();
            _renderables.Clear();
            _renderables.AddRange(renderables);
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
            material.SetUniform(pair.Key, pair.Value);
        }
    }

    public void Dispose()
    {
        _cacheShaders.Dispose();
        foreach (var fb in _fbs.Values)
        {
            fb.Dispose(_device);
        }
        _ctsWorker?.Cancel();
    }

    IRenderDevice _device;
    Dictionary<string, FrameBuffer> _fbs = new Dictionary<string, FrameBuffer>(8);
    Dictionary<string, MaterialUniform> _globalUniforms = new Dictionary<string, MaterialUniform>(32);

    List<IPipelineStage> _stageList = new List<IPipelineStage>(8);
    Dictionary<string, IPipelineStage> _stageMap = new Dictionary<string, IPipelineStage>(8);
    bool _built = false;

    ShaderCache _cacheShaders;
    TextureManager _mgrTextures;
    MaterialSet _materials;
    VertexStreamCache _cacheVertStreams;

    CancellationTokenSource? _ctsWorker;
    Thread? _worker;
    ManualResetEventSlim? _evTaskStart;
    ManualResetEventSlim? _evTaskComplete;
    List<IRenderable> _renderables = new List<IRenderable>(0);
    object _mutexBuffers = new object();
    CommandBuffer[] _cmdBuffers = new CommandBuffer[2];
    volatile int _completeBufferIndex;
}