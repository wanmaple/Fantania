using System.Numerics;
using Avalonia;
using Fantania.Models;
using FantaniaLib;

namespace Fantania.Views;

public class LevelEditorContext
{
    public Workspace Workspace => _canvas.Workspace!;
    public Camera2D Camera => _canvas.Camera!;
    public LevelEditConfig EditConfig { get; private set; }
    
    public bool FixCamera { get; set; } = false;

    public LevelEditorContext(ILevelCanvas canvas, LevelEditConfig config)
    {
        _canvas = canvas;
        EditConfig = config;
        Camera.SetZoomRange(config.ZoomMin, config.ZoomMax);
    }

    public bool InsideCanvas(Point canvasPos)
    {
        return _canvas.Control.Bounds.Contains(canvasPos);
    }

    public Vector2 CanvasToScreen(Vector2 canvasPos)
    {
        return _canvas.CanvasPositionToScreenPosition(canvasPos);
    }

    public Vector2 ScreenToCanvas(Vector2 screenPos)
    {
        return _canvas.ScreenPositionToCanvasPosition(screenPos);
    }

    public Vector2 CanvasToWorld(Vector2 canvasPos)
    {
        return _canvas.CanvasPositionToWorldPosition(canvasPos);
    }

    public Vector2 WorldToCanvas(Vector2 worldPos)
    {
        return _canvas.WorldPositionToCanvasPosition(worldPos);
    }

    public void AddCommand(ICanvasCommand command)
    {
        _canvas.AddCommand(command);
    }

    public void Focus()
    {
        _canvas.FocusSelf();
    }

    ILevelCanvas _canvas;
}