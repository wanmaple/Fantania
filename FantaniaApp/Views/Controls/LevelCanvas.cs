using System;
using System.Numerics;
using System.Threading;
using Avalonia.Controls;
using FantaniaLib;

namespace Fantania.Views;

public class LevelCanvas : GLCanvas
{
    public IWorkspace? Workspace => DataContext as IWorkspace;

    protected override void OnContextInitializing(GLDevice device)
    {
        Workspace!.LogModule.LogOptional(GlVersion.ToString());
        _buffer = new DoubleBuffer(device, CanvasWidth, CanvasHeight);
        _sync = new RenderSync();
        _blitStream = device.CreateVertexStream(VertexAnalyzer.GenerateDescriptor<VertexStandard>());
        VertexStandard[] quadVerts = [
            new VertexStandard { Position = Vector3.Zero, UV = Vector2.Zero, },
            new VertexStandard { Position = Vector3.UnitX, UV = Vector2.UnitX, },
            new VertexStandard { Position = new Vector3(Vector2.One, 0.0f), UV = Vector2.One, },
            new VertexStandard { Position = Vector3.UnitY, UV = Vector2.UnitY, },
        ];
        ushort[] quadIndices = [0, 1, 2, 0, 2, 3];
        _quad = Mesh.From(quadVerts, quadIndices);
        _blitStream.TryAppend(_quad);
        device.SyncVertexStream(_blitStream);
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
            Shader = device.CompileShader(vertSrc, fragSrc)!,
        };
        StartWorkerThread(device);
    }

    protected override void OnContextFinalizing(GLDevice device)
    {
        _buffer!.Dispose();
        _sync!.Dispose();
    }

    protected override void OnRendering(GLDevice device, int finalFbo)
    {
        _sync!.MarkFrameCollected();
        using (_sync!.AcquireGLContext())
        {
            DisplayPreviousFrame(device);
        }
        if (_sync.WaitForFrameRendered(0))
        {
            _buffer!.Swap();
            _sync.ResetFrameRendered();
        }
    }

    void DisplayPreviousFrame(GLDevice device)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        double factor = topLevel!.RenderScaling;
        int exactWidth = (int)(Bounds.Width * factor);
        int exactHeight = (int)(Bounds.Height * factor);
        device.Viewport(0, 0, exactWidth, exactHeight);
        float designRatio = (float)CanvasWidth / CanvasHeight;
        float controlRatio = (float)Bounds.Width / (float)Bounds.Height;
        float s, t;
        if (controlRatio >= designRatio)
        {
            s = 1.0f;
            t = CanvasWidth / controlRatio / CanvasHeight;
            // gl.BlitFramebuffer(0, 0, ColorBufferWidth, (int)(ColorBufferWidth / controlRatio), 0, 0, exactWidth, exactHeight, GL_COLOR_BUFFER_BIT, GL_LINEAR);
        }
        else
        {
            s = CanvasHeight * controlRatio / CanvasWidth;
            t = 1.0f;
            // gl.BlitFramebuffer(0, 0, (int)(ColorBufferHeight * controlRatio), ColorBufferHeight, 0, 0, exactWidth, exactHeight, GL_COLOR_BUFFER_BIT, GL_LINEAR);
        }
        if (UpdateUVs(Vector2.Zero, new Vector2(s, 0.0f), new Vector2(s, t), new Vector2(0.0f, t)))
        {
            _blitStream!.Reset();
            _blitStream.TryAppend(_quad!);
            device.SyncVertexStream(_blitStream);
        }
        _matFinalBlit!.SetTexture("uMainTexture", 0, _buffer!.BackBuffer.ColorAttachment);
        device.Draw(_blitStream!, _matFinalBlit);
    }

    bool UpdateUVs(params Vector2[] uvs)
    {
        bool changed = false;
        for (int i = 0; i < 4; i++)
        {
            VertexStandard vert = _quad!.GetVerticeAt<VertexStandard>(i);
            if (vert.UV != uvs[i])
            {
                vert.UV = uvs[i];
                _quad!.SetVerticeAt(i, vert);
                changed = true;
            }
        }
        return changed;
    }

    void StartWorkerThread(GLDevice device)
    {
        _ctsWorker = new CancellationTokenSource();
        _worker = new Thread(() =>
        {
            Console.WriteLine("Rendering Thread Starts.");
            while (!_ctsWorker.IsCancellationRequested)
            {
                try
                {
                    if (!_sync!.WaitForNewFrame(-1))
                        continue;

                    using (_sync.AcquireGLContext())
                    {
                        device.SetRenderTarget(_buffer!.CurrentBuffer.ID);
                        device.ClearColor("#7f7f7f".ToVector4());
                    }
                    _sync.MarkFrameRendered();
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

    DoubleBuffer? _buffer;
    RenderSync? _sync;
    VertexStream? _blitStream;
    Mesh? _quad;
    RenderMaterial? _matFinalBlit;
    Thread? _worker;
    CancellationTokenSource? _ctsWorker;
}