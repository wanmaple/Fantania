using System;
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
        ChangeModeDependsOnPlacementMode(e);
    }

    void OnMouseExited(object? sender, ControlInputEventArgs e)
    {
        _manager.SetEditorMode(LevelEditorModes.None, _context, e);
    }

    void OnMouseCaptureLost(object? sender, ControlInputEventArgs e)
    {
        _manager.SetEditorMode(LevelEditorModes.None, _context, e);
    }

    void OnMousePressed(object? sender, ControlInputEventArgs e)
    {
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMousePressed(_context, e);
    }

    void OnMouseReleased(object? sender, ControlInputEventArgs e)
    {
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMouseReleased(_context, e);
    }

    void OnMouseMoved(object? sender, ControlInputEventArgs e)
    {
        Vector2 posToCanvas = e.MouseState.Position.ToVector2();
        Vector2 posToWorld = _context.CanvasToWorld(posToCanvas);
        _context.Workspace.EditorModule.MouseWorldPosition = posToWorld.ToGridSpace(_context.EditConfig.GridAlign);
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMouseMoved(_context, e);
    }

    void OnMouseWheelChanged(object? sender, ControlInputEventArgs e)
    {
        Vector2 mouseWorldPos = _context.CanvasToWorld(e.MouseState.Position.ToVector2());
        _context.Camera.ZoomAt((float)e.MouseState.WheelDelta.Y * _context.EditConfig.ZoomSensitivity, mouseWorldPos);
        _context.Workspace.UserTemporary.CameraZoom = _context.Camera.Zoom;
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMouseWheelChanged(_context, e);
    }

    void OnKeyDown(object? sender, ControlInputEventArgs e)
    {
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnKeyDown(_context, e);
    }

    void OnKeyUp(object? sender, ControlInputEventArgs e)
    {
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnKeyUp(_context, e);
    }

    void ChangeModeDependsOnPlacementMode(ControlInputEventArgs e)
    {
        switch (_context.Workspace.EditorModule.CurrentPlacementMode)
        {
            case EntityPlacementModes.Select:
                _manager.SetEditorMode(LevelEditorModes.View, _context, e);
                break;
            case EntityPlacementModes.Place:
                _manager.SetEditorMode(LevelEditorModes.Place, _context, e);
                break;
            case EntityPlacementModes.DrawRect:
                _manager.SetEditorMode(LevelEditorModes.View, _context, e);
                break;
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