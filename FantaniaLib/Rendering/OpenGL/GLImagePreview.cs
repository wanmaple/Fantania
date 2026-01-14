using System.Numerics;
using Avalonia;
using Avalonia.Controls;

namespace FantaniaLib;

public class GLImagePreview : GLCanvas
{
    public static readonly StyledProperty<ITexture2D?> TextureProperty = AvaloniaProperty.Register<GLImagePreview, ITexture2D?>(nameof(Texture), defaultValue: null);
    public ITexture2D? Texture
    {
        get => GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }

    public static readonly DirectProperty<GLImagePreview, double> AspectRatioProperty = AvaloniaProperty.RegisterDirect<GLImagePreview, double>(nameof(AspectRatio), o => o.AspectRatio);
    double _aspectRatio = 1.0;
    public double AspectRatio => _aspectRatio;

    public GLImagePreview()
    {
        TextureProperty.Changed.AddClassHandler<GLImagePreview>((control, args) => control.OnTextureChanged((ITexture2D?)args.NewValue));
    }

    void OnTextureChanged(ITexture2D? newTex)
    {
        double oldRatio = _aspectRatio;
        if (newTex != null && newTex.IsValid)
            _aspectRatio = (double)newTex.TextureRect.Width / newTex.TextureRect.Height;
        else
            _aspectRatio = 1.0f;
        InvalidateMeasure();
        RequestNextFrameRendering();
        RaisePropertyChanged(AspectRatioProperty, oldRatio, _aspectRatio);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Texture == null || !Texture.IsValid || double.IsInfinity(_aspectRatio))
            return base.MeasureOverride(availableSize);
        double width = availableSize.Width;
        double height = availableSize.Height;
        if (!double.IsInfinity(width) && !double.IsInfinity(height))
        {
            double widthByHeight = height * _aspectRatio;
            double heightByWidth = width / _aspectRatio;
            if (widthByHeight <= width)
                width = widthByHeight;
            else
                height = heightByWidth;
        }
        else if (!double.IsInfinity(width))
            height = width / _aspectRatio;
        else if (!double.IsInfinity(height))
            width = height * _aspectRatio;
        else
        {
            width = Texture.Width;
            height = Texture.Height;
        }
        return new Size(width, height);
    }

    protected override void OnContextInitializing(ConfigurableRenderPipeline pipeline)
    {
        IRenderDevice device = pipeline.Device;
        var vertDesc = VertexAnalyzer.GenerateDescriptor<PositionUV>();
        _blitVertStream = device.CreateVertexStream(vertDesc, vertDesc.SizeofVertex * 4, sizeof(ushort) * 6);
        _quad = MeshBuilder.CreateScreenQuad();
        _blitVertStream.TryAppend(_quad);
        device.SyncVertexStream(_blitVertStream);
        string vertSrc = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/vert_fullscreen.vs");
        string fragSample = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/frag_sample.fs");
        _matSample = new RenderMaterial
        {
            State = new RenderState
            {
                DepthTestEnabled = false,
                DepthWriteEnabled = false,
                BlendingEnabled = false,
            },
            Shader = pipeline.ShaderCache.Acquire(vertSrc, fragSample)!,
        };
    }

    protected override void OnContextFinalizing(ConfigurableRenderPipeline pipeline)
    {
        IRenderDevice device = pipeline.Device;
        _blitVertStream!.Dispose(device);
        _quad!.Dispose(device);
        pipeline.ShaderCache.Release(_matSample!.Shader);
        if (_lastTexture != null)
        {
            pipeline.TextureManager.ReleaseTexture(_lastTexture);
            _lastTexture = null;
        }
    }

    protected override void OnRendering(ConfigurableRenderPipeline pipeline, int finalFbo)
    {
        IRenderDevice device = pipeline.Device;
        device.SetRenderTarget(finalFbo);
        if (device.IsFrameBufferReady())
        {
            device.ClearColor("#000000".ToVector4());
            device.ClearBufferBits(BufferBits.Color);
            var topLevel = TopLevel.GetTopLevel(this);
            double factor = topLevel!.RenderScaling;
            int exactWidth = (int)(Bounds.Width * factor);
            int exactHeight = (int)(Bounds.Height * factor);
            device.Viewport(0, 0, exactWidth, exactHeight);
            if (_lastTexture != null && Texture != _lastTexture)
            {
                pipeline.TextureManager.ReleaseTexture(_lastTexture);
                _lastTexture = null;
            }
            if (Texture != null && Texture.IsValid)
            {
                float u0 = (float)Texture.TextureRect.Left / Texture.Width;
                float u1 = (float)Texture.TextureRect.Right / Texture.Width;
                float v0 = (float)Texture.TextureRect.Top / Texture.Height;
                float v1 = (float)Texture.TextureRect.Bottom / Texture.Height;
                if (UpdateUVs(new Vector2(u0, v0), new Vector2(u1, v0), new Vector2(u1, v1), new Vector2(u0, v1)))
                {
                    _blitVertStream!.Reset();
                    _blitVertStream.TryAppend(_quad!);
                    device.SyncVertexStream(_blitVertStream);
                }
                int texId = pipeline.TextureManager.AcquireTextureID(Texture);
                _matSample!.SetTexture("u_Texture", 0, texId);
                device.Draw(_blitVertStream!, _matSample);
            }
            _lastTexture = Texture;
        }
    }

    bool UpdateUVs(params Vector2[] uvs)
    {
        bool changed = false;
        for (int i = 0; i < 4; i++)
        {
            PositionUV vert = _quad!.GetVerticeAt<PositionUV>(i);
            if (vert.UV != uvs[i])
            {
                vert.UV = uvs[i];
                _quad!.SetVerticeAt(i, vert);
                changed = true;
            }
        }
        return changed;
    }

    VertexStream? _blitVertStream;
    Mesh? _quad;
    RenderMaterial? _matSample;
    ITexture2D? _lastTexture;
}