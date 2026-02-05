using System.Numerics;
using Avalonia;
using Avalonia.Media;

namespace FantaniaLib;

public class ScaleGizmo : ITransformGizmo
{
    public bool Dragging { get; set; }
    public Vector2 ScaleFactor { get; set; } = Vector2.One;

    public bool HitTest(Point pt)
    {
        return Dragging || HoverTest(pt) != TransformGizmoHandles.None;
    }

    public TransformGizmoHandles HoverTest(Point pt)
    {
        double half = HANDLE_SIZE * 0.5;
        double xOffset = HANDLE_OFFSET * ScaleFactor.X;
        double yOffset = HANDLE_OFFSET * ScaleFactor.Y;
        var centerRect = new Rect(-half, -half, HANDLE_SIZE, HANDLE_SIZE);
        var xRect = new Rect(xOffset - half, -half, HANDLE_SIZE, HANDLE_SIZE);
        var yRect = new Rect(-half, yOffset - half, HANDLE_SIZE, HANDLE_SIZE);
        if (centerRect.Contains(pt))
            return TransformGizmoHandles.Center;
        if (xRect.Contains(pt))
            return TransformGizmoHandles.AxisX;
        if (yRect.Contains(pt))
            return TransformGizmoHandles.AxisY;
        return TransformGizmoHandles.None;
    }

    public void Render(DrawingContext context, TransformGizmoHandles hovered)
    {
        Point center = new Point(0.0, 0.0);
        var axisXEnd = new Point(HANDLE_OFFSET * ScaleFactor.X, 0.0);
        var axisYEnd = new Point(0.0, HANDLE_OFFSET * ScaleFactor.Y);
        DrawAxis(context, center, axisXEnd, AxisXColor, 2 + (hovered == TransformGizmoHandles.AxisX ? 2 : 0));
        DrawAxis(context, center, axisYEnd, AxisYColor, 2 + (hovered == TransformGizmoHandles.AxisY ? 2 : 0));
        DrawSquareHandle(context, axisXEnd, HANDLE_SIZE + (hovered == TransformGizmoHandles.AxisX ? 4 : 0), AxisXColor);
        DrawSquareHandle(context, axisYEnd, HANDLE_SIZE + (hovered == TransformGizmoHandles.AxisY ? 4 : 0), AxisYColor);
        DrawSquareHandle(context, center, HANDLE_SIZE + (hovered == TransformGizmoHandles.Center ? 4 : 0), CenterColor);
    }

    void DrawAxis(DrawingContext context, Point start, Point end, IBrush color, double thickness)
    {
        context.DrawLine(new Pen(color, thickness), start, end);
    }

    void DrawSquareHandle(DrawingContext context, Point center, double size, IBrush color)
    {
        var rect = new Rect(center.X - size * 0.5, center.Y - size * 0.5, size, size);
        context.DrawRectangle(color, null, rect);
    }

    const double HANDLE_OFFSET = 50;
    const double HANDLE_SIZE = 10;
    static readonly IBrush CenterColor = new SolidColorBrush(Colors.Blue);
    static readonly IBrush AxisXColor = new SolidColorBrush(Colors.Red);
    static readonly IBrush AxisYColor = new SolidColorBrush(Colors.Green);
}