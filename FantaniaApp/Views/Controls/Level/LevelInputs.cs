using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Input;
using Fantania.Models;
using FantaniaLib;

namespace Fantania.Views;

public class LevelInputs : IDisposable
{
    public LevelInputs(ILevelCanvas canvas, LevelEditConfig config)
    {
        _inputTracker = new ControlInputTracker(canvas.Control);
        _inputTracker.MouseEntered += OnMouseEntered;
        _inputTracker.MouseExited += OnMouseExited;
        _inputTracker.MousePressed += OnMousePressed;
        _inputTracker.MouseReleased += OnMouseReleased;
        _inputTracker.MouseMoved += OnMouseMoved;
        _inputTracker.MouseWheelChanged += OnMouseWheelChanged;
        _inputTracker.MouseCaptureLost += OnMouseCaptureLost;
        _inputTracker.KeyDown += OnKeyDown;
        _inputTracker.KeyUp += OnKeyUp;
        _context = new LevelEditorContext(canvas, config);
    }

    void OnMouseEntered(object? sender, ControlInputEventArgs e)
    {
        UserPlacement? active = _context.Workspace.PlacementModule.ActivePlacement;
        if (active != null)
            _manager.SetEditorMode(LevelEditorModes.Place, _context);
        else
            _manager.SetEditorMode(LevelEditorModes.View, _context);
    }

    void OnMouseExited(object? sender, ControlInputEventArgs e)
    {
        _manager.SetEditorMode(LevelEditorModes.None, _context);
    }

    void OnMouseCaptureLost(object? sender, ControlInputEventArgs e)
    {
        _manager.SetEditorMode(LevelEditorModes.None, _context);
    }

    void OnMousePressed(object? sender, ControlInputEventArgs e)
    {
        _manager.CurrentMode.OnMousePressed(_context, e);
    }

    void OnMouseReleased(object? sender, ControlInputEventArgs e)
    {
        _manager.CurrentMode.OnMouseReleased(_context, e);
    }

    void OnMouseMoved(object? sender, ControlInputEventArgs e)
    {
        Vector2 posToCanvas = e.MouseState.Position.ToVector2();
        Vector2 posToWorld = _context.CanvasToWorld(posToCanvas);
        _context.Workspace.EditorModule.MouseWorldPosition = posToWorld.ToGridSpace(_context.EditConfig.GridAlign);
        _manager.CurrentMode.OnMouseMoved(_context, e);
    }

    void OnMouseWheelChanged(object? sender, ControlInputEventArgs e)
    {
        Vector2 mouseWorldPos = _context.CanvasToWorld(e.MouseState.Position.ToVector2());
        _context.Camera.ZoomAt((float)e.MouseState.WheelDelta.Y * _context.EditConfig.ZoomSensitivity, mouseWorldPos);
        _manager.CurrentMode.OnMouseWheelChanged(_context, e);
    }

    void OnKeyDown(object? sender, ControlInputEventArgs e)
    {
        _manager.CurrentMode.OnKeyDown(_context, e);
    }

    void OnKeyUp(object? sender, ControlInputEventArgs e)
    {
        _manager.CurrentMode.OnKeyUp(_context, e);
        if (e.KeyState.JustReleased == Key.F)
        {
            Vector2 worldPos = _context.CanvasToWorld(e.MouseState.Position.ToVector2());
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
            var uniforms = new DesiredUniformMap();
            uniforms.SetUniform("u_Texture", new DesiredUniformValue
            {
                Type = UniformTypes.Texture,
                Value = texDef,
            });
            _context.AddCommand(new AddRenderableCommand(new RenderInfo
            {
                Anchor = Vector2.Zero,
                Color = Vector4.One,
                Depth = 2000,
                Position = worldPos,
                Rotation = 0.0f,
                Scale = Vector2.One,
                Stage = "Transparent",
                MaterialKey = "Standard",
                Size = Vector2.One * 512,
                Uniforms = uniforms,
            }));
        }
    }

    public void Dispose()
    {
        _inputTracker.Dispose();
    }

    ControlInputTracker _inputTracker;
    LevelEditorContext _context;
    LevelEditorModeManager _manager = new LevelEditorModeManager();
}