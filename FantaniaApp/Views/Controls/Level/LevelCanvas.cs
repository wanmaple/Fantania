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

    public LevelEditConfig EditConfig => _inputs!.EditConfig;

    public LevelCanvas()
    {
        Focusable = true;
    }

    protected override void OnContextInitializing(ConfigurableRenderPipeline pipeline)
    {
        Workspace!.Log(GlVersion.ToString());
        Workspace!.Log($"{pipeline.Device.GetString(GLConstants.GL_VENDOR)} ({pipeline.Device.GetString(GLConstants.GL_VERSION)})");
        RenderPipelineConfig rpConfig = Workspace!.ScriptingModule.GetCustomRenderPipelineConfigOrDefault();
        pipeline.Build(rpConfig, Workspace);
        _lvMeta = Workspace.LevelModule.Metadata;
        ColorSize = rpConfig.Resolution.ToVector2();
        _camera = new Camera2D(rpConfig.Resolution)
        {
            Position = Workspace.UserTemporary.CameraPosition,
            Zoom = Workspace.UserTemporary.CameraZoom
        };
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
        _shaderFinalBlit = pipeline.ShaderCache.Acquire(vertSrc, fragSrc);
        pipeline.StartWorkerThread(Workspace, _camera!);
        LevelEditConfig leConfig = Workspace.ScriptingModule.GetCustomLevelEditConfigOrDefault();
        _inputs = new LevelInputs(this, leConfig);
        _context = new LevelSpaceContext(this);
        _lifeOfRenderables = new RenderableLifePeriod(Workspace, pipeline);
        _lifeOfRenderables.Register(_context.RenderableHierarchy);
        if (Workspace.LevelModule.CurrentLevel != null)
            InitializeLevel(Workspace.LevelModule.CurrentLevel);
        Workspace.LevelModule.EntityAdded += OnEntityAdded;
        Workspace.LevelModule.EntityRemoved += OnEntityRemoved;
        Workspace.LevelModule.PropertyChanged += OnLevelChanged;
        Workspace.LevelModule.LayerManager.LayerVisibilityChanged += OnLayerVisibilityChanged;
    }

    protected override void OnContextFinalizing(ConfigurableRenderPipeline pipeline)
    {
        Workspace!.LevelModule.EntityAdded -= OnEntityAdded;
        Workspace.LevelModule.EntityRemoved -= OnEntityRemoved;
        Workspace.LevelModule.PropertyChanged -= OnLevelChanged;
        Workspace.LevelModule.LayerManager.LayerVisibilityChanged -= OnLayerVisibilityChanged;
        if (Workspace.LevelModule.CurrentLevel != null)
            FinalizeLevel(Workspace.LevelModule.CurrentLevel);
        _lifeOfRenderables!.Unregister(_context!.RenderableHierarchy);
        IRenderDevice device = pipeline.Device;
        _blitVertStream!.Dispose(device);
        _quad!.Dispose();
        pipeline.ShaderCache.Release(_shaderFinalBlit!);
        _inputs!.Dispose();
    }

    protected override void OnRendering(ConfigurableRenderPipeline pipeline, int finalFbo)
    {
        if (_entitiesToUpdate.Count > 0)
        {
            foreach (var entity in _entitiesToUpdate)
            {
                AddCommand(new UpdateLevelEntityCommand(entity));
            }
            _entitiesToUpdate.Clear();
        }
        HandleCanvasCommands(pipeline);
        SetupGlobalUniforms(pipeline);
        IRenderDevice device = pipeline.Device;
        FrameBuffer fbColor = pipeline.GetFrameBuffer(ConfigurableRenderPipeline.COLOR_BUFFER)!;
        device.SetRenderTargets(fbColor.ID, fbColor.ColorAttachments.Count);
        if (device.IsFrameBufferReady())
        {
            if (_context!.SceneDirty)
            {
                var renderables = _context!.CollectRenderables();
                pipeline.ReceiveRenderables(renderables);
                _context.SceneDirty = false;
            }
            device.ClearColor("#3f3f3f".ToVector4());
            // 清除DepthBuffer需要设置DepthMask为true
            device.ApplyRenderState(new RenderState
            {
                DepthTestEnabled = false,
                DepthWriteEnabled = true,
                BlendingEnabled = false,
            });
            device.ClearBufferBits(BufferBits.Color | BufferBits.Depth | BufferBits.Stencil);
            device.Viewport(0, 0, fbColor.Description.Width, fbColor.Description.Height);
            pipeline.ResetStatistics();
            pipeline.ExecuteCompletedBuffer();
            Workspace!.EditorModule.DrawCalls = pipeline.Statistics.DrawCalls;
            Workspace.EditorModule.Triangles = pipeline.Statistics.Triangles;
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
        // 如果是通过GhostEntity放置的话，就没必要再设置一遍，但是如果用Undo来添加的话就需要，因此需要检查一下是否存在
        if (!_context!.EntityManager.HasEntity(entity))
        {
            AddCommand(new SetupLevelEntityCommand(entity, EntitySetups.Add));
        }
        entity.RenderingDirty += OnEntityUpdated;
    }

    void OnEntityRemoved(LevelEntity entity)
    {
        // 和OnEntityAdded的逻辑类似
        if (_context!.EntityManager.HasEntity(entity))
        {
            AddCommand(new SetupLevelEntityCommand(entity, EntitySetups.Remove));
        }
        entity.RenderingDirty -= OnEntityUpdated;
    }

    void OnEntityUpdated(LevelEntity entity)
    {
        _entitiesToUpdate.Add(entity);
    }

    void OnLevelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LevelModule.CurrentLevel))
        {
            LevelModule module = (LevelModule)sender!;
            InitializeLevel(module.CurrentLevel);
            _lvMeta = module.Metadata;
        }
    }

    void OnLayerVisibilityChanged()
    {
        _context!.SceneDirty = true;
        var selections = Workspace!.EditorModule.SelectedObjects;
        var toRm = new HashSet<ISelectableItem>(8);
        foreach (var sel in selections)
        {
            if (sel is LevelEntityNode node)
            {
                var owner = (MultiNodesEntity)node.Owner;
                if (!Workspace.LevelModule.LayerManager.IsLayerVisible(owner.Layer))
                {
                    toRm.Add(sel);
                }
            }
            else
            {
                LevelEntity entity = (LevelEntity)sel;
                if (!Workspace.LevelModule.LayerManager.IsLayerVisible(entity.Layer))
                {
                    toRm.Add(sel);
                }
            }
        }
        foreach (var sel in toRm)
        {
            selections.Remove(sel);
        }
    }

    void InitializeLevel(IReadonlyLevel? lv)
    {
        _context!.RenderableHierarchy.Clear();
        _context.SelectableHierarchy.Clear();
        if (lv != null)
        {
            foreach (var entity in lv.Entities)
            {
                AddCommand(new SetupLevelEntityCommand(entity, EntitySetups.Add));
                entity.RenderingDirty += OnEntityUpdated;
            }
        }
    }

    void FinalizeLevel(IReadonlyLevel? lv)
    {
        if (lv != null)
        {
            foreach (var entity in lv.Entities)
            {
                entity.RenderingDirty -= OnEntityUpdated;
            }
        }
        _context!.RenderableHierarchy.Clear();
        _context.SelectableHierarchy.Clear();
    }

    void SetupGlobalUniforms(ConfigurableRenderPipeline pipeline)
    {
        pipeline.GlobalUniforms.SetUniform("u_Time", Workspace!.Time);
        pipeline.GlobalUniforms.SetUniform("u_View", _camera!.ViewMatrix);
        pipeline.GlobalUniforms.SetUniform("u_Resolution", new Vector4(ColorSize.X, ColorSize.Y, 1.0f / ColorSize.X, 1.0f / ColorSize.Y));
        pipeline.GlobalUniforms.SetUniform("u_EnvArgs", new Vector4(_lvMeta!.AmbientColor.X, _lvMeta.AmbientColor.Y, _lvMeta.AmbientColor.Z, 0.0f));
        Vector3 envLightDir = Vector3.Normalize(_lvMeta.EnvironmentLightDirection);
        pipeline.GlobalUniforms.SetUniform("u_EnvLight", new Vector4(envLightDir.X, envLightDir.Y, envLightDir.Z, _lvMeta.EnvironmentLightIntensity));
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
        _uniformsFinalBlit.SetUniform("u_MainTexture", TextureDefinition.CreateGpuDefinition(fbColor.ColorAttachment), 0);
        device.ApplyRenderState(_blitState!.Value);
        device.Draw(_blitVertStream!, _shaderFinalBlit!, _uniformsFinalBlit);
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

    public Vector2 CanvasPositionToScreenPosition(Vector2 canvasPos)
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

    public Vector2 ScreenPositionToCanvasPosition(Vector2 screenPos)
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

    public Vector2 CanvasPositionToWorldPosition(Vector2 canvasPos)
    {
        Vector2 posToScreen = CanvasPositionToScreenPosition(canvasPos);
        return _camera!.ScreenPositionToWorldPosition(posToScreen);
    }

    public Vector2 WorldPositionToCanvasPosition(Vector2 worldPos)
    {
        if (_camera == null) return worldPos;
        Vector2 posToScreen = _camera.WorldPositionToScreenPosition(worldPos);
        return ScreenPositionToCanvasPosition(posToScreen);
    }

    public Vector2 CanvasMovementToScreenMovement(Vector2 canvasVec)
    {
        float canvasWidth = ControlSize.X;
        float canvasHeight = ControlSize.Y;
        float designRatio = (float)_camera!.Viewport.X / _camera.Viewport.Y;
        float canvasRatio = canvasWidth / canvasHeight;
        if (canvasRatio >= designRatio)
        {
            float x = canvasVec.X / canvasWidth * _camera!.Viewport.X;
            float h = canvasWidth / designRatio;
            float y = canvasVec.Y / h * _camera!.Viewport.Y;
            return new Vector2(x, y);
        }
        else
        {
            float w = canvasHeight * designRatio;
            float x = canvasVec.X / w * _camera!.Viewport.X;
            float y = canvasVec.Y / canvasHeight * _camera!.Viewport.Y;
            return new Vector2(x, y);
        }
    }

    public Vector2 ScreenMovementToCanvasMovement(Vector2 screenVec)
    {
        throw new System.NotImplementedException();
    }

    public Vector2 CanvasMovementToWorldMovement(Vector2 canvasVec)
    {
        Vector2 screenVec = CanvasMovementToScreenMovement(canvasVec);
        return _camera!.ScreenMovementToWorldMovement(screenVec);
    }

    public Vector2 WorldMovementToCanvasMovement(Vector2 worldVec)
    {
        return Vector2.Zero;
    }

    public void AddCommand(ICanvasCommand command)
    {
        _commands.Add(command);
    }

    public void FocusSelf()
    {
        Focus();
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
    ShaderProgram? _shaderFinalBlit;
    UniformSet _uniformsFinalBlit = new UniformSet();
    LevelInputs? _inputs;
    LevelSpaceContext? _context;
    List<ICanvasCommand> _commands = new List<ICanvasCommand>(0);
    RenderableLifePeriod? _lifeOfRenderables;
    LevelMetadata? _lvMeta;

    HashSet<LevelEntity> _entitiesToUpdate = new HashSet<LevelEntity>(8);
}