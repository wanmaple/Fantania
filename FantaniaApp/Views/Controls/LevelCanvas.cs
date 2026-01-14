using System;
using System.Numerics;
using System.Threading;
using Avalonia.Controls;
using FantaniaLib;

namespace Fantania.Views;

public class LevelCanvas : GLCanvas
{
    public Workspace? Workspace => DataContext as Workspace;

    protected override void OnContextInitializing(ConfigurableRenderPipeline pipeline)
    {
        Workspace!.LogModule.LogOptional(GlVersion.ToString());
        RenderPipelineConfig rpConfig = Workspace.ScriptingModule.GetCustomRenderPipelineConfigOrDefault();
        pipeline.Build(rpConfig);
        IRenderDevice device = pipeline.Device;
        var vertDesc = VertexAnalyzer.GenerateDescriptor<PositionUV>();
        _blitVertStream = device.CreateVertexStream(vertDesc, vertDesc.SizeofVertex * 4, sizeof(ushort) * 6);
        _quad = MeshBuilder.CreateScreenQuad();
        _blitVertStream.TryAppend(_quad);
        device.SyncVertexStream(_blitVertStream);
        string vertSrc = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/vert_fullscreen.vs");
        string fragSrc = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/frag_finalblit.fs");
        _matFinalBlit = new RenderMaterial
        {
            State = new RenderState
            {
                DepthTestEnabled = false,
                DepthWriteEnabled = false,
                BlendingEnabled = false,
            },
            Shader = pipeline.ShaderCache.Acquire(vertSrc, fragSrc)!,
        };
        // StartWorkerThread();
    }

    protected override void OnContextFinalizing(ConfigurableRenderPipeline pipeline)
    {
        IRenderDevice device = pipeline.Device;
        _blitVertStream!.Dispose(device);
        _quad!.Dispose(device);
        pipeline.ShaderCache.Release(_matFinalBlit!.Shader);
        // _ctsWorker!.Cancel();
    }

    protected override void OnRendering(ConfigurableRenderPipeline pipeline, int finalFbo)
    {
        SetupGlobalUniforms(pipeline);
        IRenderDevice device = pipeline.Device;
        FrameBuffer fbColor = pipeline.GetFrameBuffer(ConfigurableRenderPipeline.COLOR_BUFFER)!;
        device.SetRenderTarget(fbColor.ID);
        if (device.IsFrameBufferReady())
        {
            device.ClearColor("#FF000000".ToVector4());
            device.ClearBufferBits(BufferBits.Color | BufferBits.Depth);
            device.Viewport(0, 0, fbColor.Description.Width, fbColor.Description.Height);
        }
        device.SetRenderTarget(finalFbo);
        if (device.IsFrameBufferReady())
            BlitColorToTarget(device, fbColor);
        RequestNextFrameRendering();
    }

    void SetupGlobalUniforms(ConfigurableRenderPipeline pipeline)
    {
        pipeline.SetGlobalUniform("u_Time", Workspace!.Time);
    }

    void BlitColorToTarget(IRenderDevice device, FrameBuffer fbColor)
    {
        device.ClearBufferBits(BufferBits.Color);
        var topLevel = TopLevel.GetTopLevel(this);
        double factor = topLevel!.RenderScaling;
        int exactWidth = (int)(Bounds.Width * factor);
        int exactHeight = (int)(Bounds.Height * factor);
        device.Viewport(0, 0, exactWidth, exactHeight);
        int designWidth = fbColor.Description.Width, designHeight = fbColor.Description.Height;
        float designRatio = (float)designWidth / designHeight;
        float controlRatio = (float)Bounds.Width / (float)Bounds.Height;
        float s, t;
        if (controlRatio >= designRatio)
        {
            s = 1.0f;
            t = designWidth / controlRatio / designHeight;
        }
        else
        {
            s = designHeight * controlRatio / designWidth;
            t = 1.0f;
        }
        if (UpdateUVs(Vector2.Zero, new Vector2(s, 0.0f), new Vector2(s, t), new Vector2(0.0f, t)))
        {
            _blitVertStream!.Reset();
            _blitVertStream.TryAppend(_quad!);
            device.SyncVertexStream(_blitVertStream);
        }
        _matFinalBlit!.SetTexture("u_MainTexture", 0, fbColor.ColorAttachment);
        device.Draw(_blitVertStream!, _matFinalBlit!);
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

    void StartWorkerThread()
    {
        _ctsWorker = new CancellationTokenSource();
        _worker = new Thread(() =>
        {
            Console.WriteLine("Rendering Thread Starts.");
            while (!_ctsWorker.IsCancellationRequested)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Rendering Thread Exception: {ex}");
                }
            }
        })
        {
            Name = "Rendering Thread",
            IsBackground = true,
        };
        _worker.Start();
    }

    VertexStream? _blitVertStream;
    Mesh? _quad;
    RenderMaterial? _matFinalBlit;
    Thread? _worker;
    CancellationTokenSource? _ctsWorker;
}