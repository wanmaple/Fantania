using Avalonia;
using Avalonia.Media;

namespace FantaniaLib;

public class ScaleGizmo : ITransformGizmo
{
    public bool Dragging { get; set; }

    public bool HitTest(Point pt)
    {
        return Dragging || HoverTest(pt) != TransformGizmoHandles.None;
    }

    public TransformGizmoHandles HoverTest(Point pt)
    {
        return TransformGizmoHandles.None;
    }

    public void Render(DrawingContext context, TransformGizmoHandles hovered)
    {
    }
}