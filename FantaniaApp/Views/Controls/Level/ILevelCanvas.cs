using System.Numerics;
using Avalonia.Controls;
using Fantania.Models;
using FantaniaLib;

namespace Fantania.Views;

public interface ILevelCanvas
{
    Workspace? Workspace { get; }
    Camera2D? Camera { get; }
    Vector2 ColorSize { get; }
    Vector2 ControlSize { get; }
    Control Control { get; }

    void AddCommand(ICanvasCommand command);

    Vector2 CanvasToScreen(Vector2 canvasPos);
    Vector2 ScreenToCanvas(Vector2 screenPos);
    Vector2 CanvasToWorld(Vector2 canvasPos);
    Vector2 WorldToCanvas(Vector2 worldPos);
}