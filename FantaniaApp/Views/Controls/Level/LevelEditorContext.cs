using System.Numerics;
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

    public Vector2 CanvasToScreen(Vector2 canvasPos)
    {
        return _canvas.CanvasPositionToScreenPosition(canvasPos);
    }

    public Vector2 CanvasToWorld(Vector2 canvasPos)
    {
        return _canvas.CanvasPositionToWorldPosition(canvasPos);
    }

    public void AddCommand(ICanvasCommand command)
    {
        _canvas.AddCommand(command);
    }

    ILevelCanvas _canvas;
}