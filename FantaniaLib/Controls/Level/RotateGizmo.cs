using Avalonia;
using Avalonia.Media;

namespace FantaniaLib;

public class RotateGizmo : ITransformGizmo
{
    public bool Dragging { get; set; }

    public bool HitTest(Point pt)
    {
        return Dragging || HoverTest(pt) != TransformGizmoHandles.None;
    }

    public TransformGizmoHandles HoverTest(Point pt)
    {
        throw new NotImplementedException();
    }

    public void Render(DrawingContext context, TransformGizmoHandles hovered)
    {
        throw new NotImplementedException();
    }
}