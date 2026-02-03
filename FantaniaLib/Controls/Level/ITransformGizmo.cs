using Avalonia;
using Avalonia.Media;

namespace FantaniaLib;

public enum TransformGizmoTypes
{
    None,
    Translation,
    Rotation,
    Scale,
}

public enum TransformGizmoHandles
{
    None,
    Center,
    AxisX,
    AxisY,
    Rotate,
}

public interface ITransformGizmo
{
    bool Dragging { get; set; }

    bool HitTest(Point pt);
    TransformGizmoHandles HoverTest(Point pt);
    void Render(DrawingContext context, TransformGizmoHandles hovered);
}