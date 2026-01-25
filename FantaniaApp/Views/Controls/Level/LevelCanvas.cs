using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Avalonia.Controls;
using Fantania.Localization;
using Fantania.Models;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public class LevelCanvas : GLCanvas, ILevelCanvas
{
    public LevelViewModel ViewModel => (LevelViewModel)DataContext!;
    public Workspace? Workspace => ViewModel.Workspace;
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
        Workspace!.LogOptional(GlVersion.ToString());
        RenderPipelineConfig rpConfig = Workspace!.ScriptingModule.GetCustomRenderPipelineConfigOrDefault();
        pipeline.Build(rpConfig, Workspace);
        ColorSize = rpConfig.Resolution.ToVector2();
        _camera = new Camera2D(rpConfig.Resolution);
        _camera.Position = Workspace.UserTemporary.CameraPosition;
        _camera.Zoom = Workspace.UserTemporary.CameraZoom;
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
        _context = new LevelSpaceContext(this);
        _lifeOfRenderables = new RenderableLifePeriod(Workspace, pipeline);
        _lifeOfRenderables.Register(_context.SpaceHierarchy);
        if (Workspace.LevelModule.CurrentLevel != null)
            InitializeLevel(Workspace.LevelModule.CurrentLevel);
        Workspace.LevelModule.EntityAdded += OnEntityAdded;
        Workspace.LevelModule.EntityRemoved += OnEntityRemoved;
        Workspace.LevelModule.PropertyChanged += OnLevelChanged;
    }

    protected override void OnContextFinalizing(ConfigurableRenderPipeline pipeline)
    {
        Workspace!.LevelModule.EntityAdded -= OnEntityAdded;
        Workspace.LevelModule.EntityRemoved -= OnEntityRemoved;
        Workspace.LevelModule.PropertyChanged -= OnLevelChanged;
        _lifeOfRenderables!.Unregister(_context!.SpaceHierarchy);
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
            if (true /* Scene Dirty */)
            {
                var renderables = _context!.CollectRenderables();
                pipeline.ReceiveRenderables(renderables);
            }
            device.ClearColor("#FF000000".ToVector4());
            device.ClearBufferBits(BufferBits.Color | BufferBits.Depth);
            device.Viewport(0, 0, fbColor.Description.Width, fbColor.Description.Height);
            pipeline.ExecuteCompletedBuffer();
        }
        device.SetRenderTarget(finalFbo);
        if (device.IsFrameBufferReady())
            BlitColorToTarget(device, fbColor);
        RequestNextFrameRendering();
    }

    protected override void OnContextCreateFailed()
    {
        Workspace!.LogError(LocalizationHelper.GetLocalizedString("ERR_GLContextFailure"));
    }

    void OnEntityAdded(LevelEntity entity)
    {
        if (!_context!.EntityManager.HasEntity(entity))
        {
            AddCommand(new SetupLevelEntityCommand(entity, EntitySetups.Add));
        }
    }

    void OnEntityRemoved(LevelEntity entity)
    {
        if (_context!.EntityManager.HasEntity(entity))
        {
            AddCommand(new SetupLevelEntityCommand(entity, EntitySetups.Remove));
        }
    }

    void OnLevelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LevelModule.CurrentLevel))
        {
            LevelModule module = (LevelModule)sender!;
            InitializeLevel(module.CurrentLevel);
        }
    }

    void InitializeLevel(IReadonlyLevel? lv)
    {
        _context!.SpaceHierarchy.Clear();
        if (lv != null)
        {
            foreach (var entity in lv.Entities)
            {
                AddCommand(new SetupLevelEntityCommand(entity, EntitySetups.Add));
            }
        }
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
        _matFinalBlit!.Uniforms.SetUniform("u_MainTexture", TextureDefinition.CreateGpuDefinition(fbColor.ColorAttachment), 0);
        device.ApplyRenderState(_blitState!.Value);
        // device.SetupFrameBufferSRGB(true);
        device.Draw(_blitVertStream!, _matFinalBlit!);
        // device.SetupFrameBufferSRGB(false);
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

    public Vector2 ScreenToCanvas(Vector2 screenPos)
    {
        float canvasWidth = ControlSize.X;
        float canvasHeight = ControlSize.Y;
        float designRatio = (float)_camera!.Viewport.X / _camera.Viewport.Y;
        float canvasRatio = canvasWidth / canvasHeight;
        Vector2 canvasPos = Vector2.Zero;
        if (canvasRatio >= designRatio)
        {
            canvasPos.X = screenPos.X / _camera.Viewport.X * canvasWidth;
            float h = canvasWidth / designRatio;
            canvasPos.Y = canvasHeight - (_camera.Viewport.Y - screenPos.Y) / _camera.Viewport.Y * h;
        }
        else
        {
            float w = canvasHeight * designRatio;
            canvasPos.X = screenPos.X / _camera.Viewport.X * w;
            canvasPos.Y = screenPos.Y / _camera.Viewport.Y * canvasHeight;
        }
        return canvasPos;
    }

    public Vector2 CanvasToWorld(Vector2 canvasPos)
    {
        Vector2 posToScreen = CanvasToScreen(canvasPos);
        return _camera!.ScreenToWorld(posToScreen);
    }

    public Vector2 WorldToCanvas(Vector2 worldPos)
    {
        Vector2 posToScreen = _camera!.WorldToScreen(worldPos);
        return ScreenToCanvas(posToScreen);
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
    LevelSpaceContext? _context;
    List<ICanvasCommand> _commands = new List<ICanvasCommand>(0);
    RenderableLifePeriod? _lifeOfRenderables;
}