using System.Numerics;

namespace FantaniaLib;

public class ConfigurableRenderPipeline : IDisposable
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

    public ConfigurableRenderPipeline(IRenderDevice device)
    {
        _device = device;
        _cacheShaders = new ShaderCache(device);
        _mgrTextures = new TextureManager(device);
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

    public void Build(RenderPipelineConfig config)
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
        _built = true;
    }

    void AddFrameBuffer(FrameBufferConfig config)
    {
        _fbs[config.Name] = _device.CreateFrameBuffer(config.Description);
    }

    void AddStage(IPipelineStage stage)
    {
        if (_built) return;
        _stageList.Add(stage);
        _namedMap[stage.Name] = stage;
    }

    public void Dispose()
    {
        foreach (var fb in _fbs.Values)
        {
            fb.Dispose(_device);
        }
    }

    IRenderDevice _device;
    Dictionary<string, FrameBuffer> _fbs = new Dictionary<string, FrameBuffer>(8);
    Dictionary<string, MaterialUniform> _globalUniforms = new Dictionary<string, MaterialUniform>(32);

    List<IPipelineStage> _stageList = new List<IPipelineStage>(8);
    Dictionary<string, IPipelineStage> _namedMap = new Dictionary<string, IPipelineStage>(8);
    bool _built = false;

    ShaderCache _cacheShaders;
    TextureManager _mgrTextures;
}