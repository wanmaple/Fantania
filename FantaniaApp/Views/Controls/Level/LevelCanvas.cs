using System.Collections.Generic;
using System.Numerics;
using Avalonia.Controls;
using Fantania.Localization;
using Fantania.Models;
using FantaniaLib;

namespace Fantania.Views;

public class LevelCanvas : GLCanvas, ILevelCanvas
{
    public Workspace? Workspace => DataContext as Workspace;
    public Camera2D? Camera => _camera;
    public Control Control => this;
    public Vector2 ColorSize { get; private set; } = Vector2.Zero;
    public Vector2 ControlSize => new Vector2((float)Bounds.Width, (float)Bounds.Height);

    public LevelCanvas()
    {
        Focusable = true;
    }

    protected override void OnContextInitializing(ConfigurableRenderPipeline pipeline)
    {
        Workspace!.LogModule.LogOptional(GlVersion.ToString());
        RenderPipelineConfig rpConfig = Workspace.ScriptingModule.GetCustomRenderPipelineConfigOrDefault();
        pipeline.Build(rpConfig, Workspace);
        ColorSize = rpConfig.Resolution.ToVector2();
        _camera = new Camera2D(rpConfig.Resolution);
        IRenderDevice device = pipeline.Device;
        var vertDesc = VertexAnalyzer.GenerateDescriptor<PositionUV>();
        _blitVertStream = device.CreateVertexStream(vertDesc, vertDesc.SizeofVertex * 4, sizeof(ushort) * 6);
        _quad = MeshBuilder.CreateScreenQuad();
        _blitVertStream.TryAppend(_quad);
        device.SyncVertexStream(_blitVertStream);
        string vertSrc = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/vert_fullscreen.vs");
        string fragSrc = AvaloniaHelper.ReadAssetText("avares://Fantania/Assets/shaders/frag_finalblit.fs");
        _blitState = new RenderState
        {
            DepthTestEnabled = false,
            DepthWriteEnabled = false,
            BlendingEnabled = false,
        };
        _matFinalBlit = new RenderMaterial
        {
            Shader = pipeline.ShaderCache.Acquire(vertSrc, fragSrc)!,
        };
        pipeline.StartWorkerThread();
        LevelEditConfig leConfig = Workspace.ScriptingModule.GetCustomLevelEditConfigOrDefault();
        _inputs = new LevelInputs(this, leConfig);
        _context = new LevelRenderContext(this);
    }

    protected override void OnContextFinalizing(ConfigurableRenderPipeline pipeline)
    {
        IRenderDevice device = pipeline.Device;
        _blitVertStream!.Dispose(device);
        _quad!.Dispose();
        pipeline.ShaderCache.Release(_matFinalBlit!.Shader);
        _inputs!.Dispose();
    }

    protected override void OnRendering(ConfigurableRenderPipeline pipeline, int finalFbo)
    {
        HandleCanvasCommands(pipeline);
        SetupGlobalUniforms(pipeline);
        IRenderDevice device = pipeline.Device;
        FrameBuffer fbColor = pipeline.GetFrameBuffer(ConfigurableRenderPipeline.COLOR_BUFFER)!;
        device.SetRenderTarget(fbColor.ID);
        if (device.IsFrameBufferReady())
        {
            device.ClearColor("#FF000000".ToVector4());
            device.ClearBufferBits(BufferBits.Color | BufferBits.Depth);
            device.Viewport(0, 0, fbColor.Description.Width, fbColor.Description.Height);
            if (true /* Scene Dirty */)
            {
                var renderables = _context!.CollectRenderables();
                pipeline.CollectRenderables(renderables);
            }
            pipeline.ExecuteCompletedBuffer();
        }
        device.SetRenderTarget(finalFbo);
        if (device.IsFrameBufferReady())
            BlitColorToTarget(device, fbColor);
        RequestNextFrameRendering();
    }

    protected override void OnContextCreateFailed()
    {
        Workspace!.LogModule.LogError(LocalizationHelper.GetLocalizedString("ERR_GLContextFailure"));
    }

    void SetupGlobalUniforms(ConfigurableRenderPipeline pipeline)
    {
        pipeline.GlobalUniforms.SetUniform("u_Time", Workspace!.Time);
        pipeline.GlobalUniforms.SetUniform("u_View", _camera!.ViewMatrix);
        pipeline.GlobalUniforms.SetUniform("u_Resolution", new Vector4(ColorSize.X, ColorSize.Y, 1.0f / ColorSize.X, 1.0f / ColorSize.Y));
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
        _matFinalBlit!.Uniforms.SetUniform("u_MainTexture", (0, fbColor.ColorAttachment));
        device.ApplyRenderState(_blitState!.Value);
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

    public Vector2 CanvasToScreen(Vector2 canvasPos)
    {
        float canvasWidth = ControlSize.X;
        float canvasHeight = ControlSize.Y;
        float designRatio = (float)_camera!.Viewport.X / _camera.Viewport.Y;
        float canvasRatio = canvasWidth / canvasHeight;
        Vector2 screenPos = Vector2.Zero;
        if (canvasRatio >= designRatio)
        {
            screenPos.X = canvasPos.X / canvasWidth * _camera.Viewport.X;
            float h = canvasWidth / designRatio;
            screenPos.Y = (1.0f - (canvasHeight - canvasPos.Y) / h) * _camera.Viewport.Y;
        }
        else
        {
            float w = canvasHeight * designRatio;
            screenPos.X = canvasPos.X / w * _camera.Viewport.X;
            screenPos.Y = canvasPos.Y / canvasHeight * _camera.Viewport.Y;
        }
        return screenPos;
    }

    public Vector2 CanvasToWorld(Vector2 canvasPos)
    {
        Vector2 posToScreen = CanvasToScreen(canvasPos);
        return _camera!.ScreenToWorld(posToScreen);
    }

    public void AddCommand(ICanvasCommand command)
    {
        _commands.Add(command);
    }

    void HandleCanvasCommands(ConfigurableRenderPipeline pipeline)
    {
        if (_commands.Count > 0)
        {
            foreach (var cmd in _commands)
            {
                cmd.Execute(_context!, pipeline);
            }
            _commands.Clear();
        }
    }

    Camera2D? _camera;
    VertexStream? _blitVertStream;
    Mesh? _quad;
    RenderState? _blitState;
    RenderMaterial? _matFinalBlit;
    LevelInputs? _inputs;
    LevelRenderContext? _context;
    List<ICanvasCommand> _commands = new List<ICanvasCommand>(0);
}