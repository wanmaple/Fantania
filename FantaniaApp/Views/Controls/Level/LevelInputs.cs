using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Input;
using Fantania.Models;
using FantaniaLib;

namespace Fantania.Views;

public class LevelInputs : IDisposable
{
    public Camera2D Camera => _canvas.Camera!;
    public Workspace Workspace => _canvas.Workspace!;

    public int GridAlign { get; private set; }

    public LevelInputs(ILevelCanvas canvas, LevelEditConfig config)
    {
        _canvas = canvas;
        _inputTracker = new ControlInputTracker(_canvas.Control);
        _inputTracker.MousePressed += OnMousePressed;
        _inputTracker.MouseReleased += OnMouseReleased;
        _inputTracker.MouseMoved += OnMouseMoved;
        _inputTracker.MouseWheelChanged += OnMouseWheelChanged;
        _inputTracker.KeyDown += OnKeyDown;
        _inputTracker.KeyUp += OnKeyUp;
        SyncConfig(config);
    }

    void SyncConfig(LevelEditConfig config)
    {
        GridAlign = MathHelper.Clamp(config.GridAlign, 1, 32);
    }

    void OnMousePressed(object? sender, ControlInputEventArgs e)
    {
    }

    void OnMouseReleased(object? sender, ControlInputEventArgs e)
    {
    }

    void OnMouseMoved(object? sender, ControlInputEventArgs e)
    {
        Vector2 posToCanvas = e.MouseState.Position.ToVector2();
        Vector2 posToWorld = _canvas.CanvasToWorld(posToCanvas);
        Workspace.EditorModule.MouseWorldPosition = posToWorld.ToGridSpace(GridAlign);
    }

    void OnMouseWheelChanged(object? sender, ControlInputEventArgs e)
    {
    }

    void OnKeyDown(object? sender, ControlInputEventArgs e)
    {
    }

    void OnKeyUp(object? sender, ControlInputEventArgs e)
    {
        if (e.KeyState.JustReleased == Key.F)
        {
            Vector2 worldPos = _canvas.CanvasToWorld(e.MouseState.Position.ToVector2());
            // Workspace.LogModule.Log(worldPos.ToGridSpace(GridAlign).ToString());
            var texDef = new TextureDefinition
            {
                TextureType = TextureTypes.Image,
                TextureParameters = new TextureParameterUnion
                {
                    ImageParams = new ImageParameter
                    {
                        ImagePath = "textures/scene/trees/tree_1.png",
                    },
                },
            };
            _canvas.AddCommand(new AddRenderableCommand(new RenderInfo
            {
                Anchor = Vector2.Zero,
                Color = Vector4.One,
                Depth = 2000,
                Position = worldPos,
                Rotation = 0.0f,
                Scale = Vector2.One,
                Stage = "Transparent",
                MaterialKey = "SampleTexture",
                Size = Vector2.Zero,
                Uniforms = new Dictionary<string, object>
                {
                    { "u_Texture", texDef },
                },
            }));
        }
    }

    public void Dispose()
    {
        _inputTracker.Dispose();
    }

    ILevelCanvas _canvas;
    ControlInputTracker _inputTracker;
}