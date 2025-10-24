using System;
using Avalonia;
using Avalonia.Input;

namespace Fantania;

public struct MouseState
{
    public MouseButton Button { get; set; }
    public bool Moved { get; set; }
    public Point RelativePosition { get; set; }
}

public interface ICanvasInputHandler
{
    void Tick(TimeSpan dt);
    bool MouseEnter(Point relativePosition);
    bool MouseExit();
    bool MouseClick(Point relativePosition, MouseButton button, KeyModifiers modifiers);
    bool MouseDragging(Point relativePosition, Point changed, MouseButton button, KeyModifiers modifiers);
    bool MouseDraggingEnd(Point relativePosition, MouseButton button);
    bool MouseMoving(Point relativePosition, KeyModifiers modifiers);
    bool MouseScrolling(Point relativePosition, Vector delta, KeyModifiers modifiers);
    bool KeyReleased(Key key, KeyModifiers modifiers);
}