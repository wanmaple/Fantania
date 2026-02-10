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
    void FocusSelf();

    Vector2 CanvasPositionToScreenPosition(Vector2 canvasPos);
    Vector2 ScreenPositionToCanvasPosition(Vector2 screenPos);
    Vector2 CanvasPositionToWorldPosition(Vector2 canvasPos);
    Vector2 WorldPositionToCanvasPosition(Vector2 worldPos);
    Vector2 CanvasMovementToScreenMovement(Vector2 canvasVec);
    Vector2 ScreenMovementToCanvasMovement(Vector2 screenVec);
    Vector2 CanvasMovementToWorldMovement(Vector2 canvasVec);
    Vector2 WorldMovementToCanvasMovement(Vector2 worldVec);
}