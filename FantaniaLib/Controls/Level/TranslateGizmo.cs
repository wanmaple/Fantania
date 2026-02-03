using Avalonia;
using Avalonia.Media;

namespace FantaniaLib;

public class TranslateGizmo : ITransformGizmo
{
    public bool Dragging { get; set; }

    public bool HitTest(Point pt)
    {
        return Dragging || HoverTest(pt) != TransformGizmoHandles.None;
    }

    public TransformGizmoHandles HoverTest(Point pt)
    {
        double halfSize = CENTER_HANDLE_SIZE * 0.5;
        Rect ctRect = new Rect(-halfSize, -halfSize, CENTER_HANDLE_SIZE, CENTER_HANDLE_SIZE);
        Rect xRect = new Rect(halfSize, -halfSize, ARROW_LENGTH, CENTER_HANDLE_SIZE);
        Rect yRect = new Rect(-halfSize, halfSize, CENTER_HANDLE_SIZE, ARROW_LENGTH);
        if (ctRect.Contains(pt))
            return TransformGizmoHandles.Center;
        else if (xRect.Contains(pt))
            return TransformGizmoHandles.AxisX;
        else if (yRect.Contains(pt))
            return TransformGizmoHandles.AxisY;
        return TransformGizmoHandles.None;
    }

    public void Render(DrawingContext context, TransformGizmoHandles hovered)
    {
        Point center = new Point(0.0f, 0.0);
        var axisXEnd = new Point(center.X + ARROW_LENGTH, center.Y);
        DrawArrow(context, center, axisXEnd, AxisXColor, 2 + (hovered == TransformGizmoHandles.AxisX ? 2 : 0));
        var axisYEnd = new Point(center.X, center.Y + ARROW_LENGTH);
        DrawArrow(context, center, axisYEnd, AxisYColor, 2 + (hovered == TransformGizmoHandles.AxisY ? 2 : 0));
        DrawRectHandle(context, center, CENTER_HANDLE_SIZE + (hovered == TransformGizmoHandles.Center ? 4 : 0), CenterColor);
    }

    void DrawArrow(DrawingContext context, Point start, Point end, IBrush color, double thickness)
    {
        double arrowHeadSize = ARROW_HEAD_SIZE;
        var pen = new Pen(color, thickness);
        var direction = new Vector(end.X - start.X, end.Y - start.Y);
        direction = direction.Normalize();
        context.DrawLine(pen, start, end - direction * arrowHeadSize);
        var perpendicular = new Vector(-direction.Y, direction.X);
        var pt1 = new Point(
            end.X - direction.X * arrowHeadSize + perpendicular.X * arrowHeadSize * 0.5,
            end.Y - direction.Y * arrowHeadSize + perpendicular.Y * arrowHeadSize * 0.5
        );
        var pt2 = new Point(
            end.X - direction.X * arrowHeadSize - perpendicular.X * arrowHeadSize * 0.5,
            end.Y - direction.Y * arrowHeadSize - perpendicular.Y * arrowHeadSize * 0.5
        );
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(end, true);
            ctx.LineTo(pt1);
            ctx.LineTo(pt2);
            ctx.EndFigure(true);
        }
        context.DrawGeometry(color, null, geometry);
    }

    void DrawRectHandle(DrawingContext context, Point center, double size, IBrush color)
    {
        var rect = new Rect(center.X - size * 0.5, center.Y - size * 0.5, size, size);
        context.DrawRectangle(color, null, rect);
    }

    const double CENTER_HANDLE_SIZE = 12;
    const double ARROW_LENGTH = 60;
    const double ARROW_HEAD_SIZE = 12;
    static readonly IBrush CenterColor = new SolidColorBrush(Colors.Blue);
    static readonly IBrush AxisXColor = new SolidColorBrush(Colors.Red);
    static readonly IBrush AxisYColor = new SolidColorBrush(Colors.Green);
}