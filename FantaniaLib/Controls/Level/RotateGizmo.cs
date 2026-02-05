using Avalonia;
using Avalonia.Media;

namespace FantaniaLib;

public class RotateGizmo : ITransformGizmo
{
    public bool Dragging { get; set; }
    public double RotationAngle { get; set; }
    public bool ShowRay { get; set; }
    public Point RayEnd { get; set; }

    public bool HitTest(Point pt)
    {
        return Dragging || HoverTest(pt) != TransformGizmoHandles.None;
    }

    public TransformGizmoHandles HoverTest(Point pt)
    {
        double dist = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
        if (dist >= RING_RADIUS - HIT_THICKNESS && dist <= RING_RADIUS + HIT_THICKNESS)
            return TransformGizmoHandles.Rotate;
        return TransformGizmoHandles.None;
    }

    public void Render(DrawingContext context, TransformGizmoHandles hovered)
    {
        Point center = new Point(0.0, 0.0);
        double thickness = RING_THICKNESS + (hovered == TransformGizmoHandles.Rotate ? 2 : 0);
        var pen = new Pen(RingColor, thickness);
        context.DrawEllipse(null, pen, center, RING_RADIUS, RING_RADIUS);
        if (ShowRay)
            context.DrawLine(new Pen(RayColor, RAY_THICKNESS), center, RayEnd);
        DrawArrowOnRing(context, ARROW_BASE_ANGLE + RotationAngle, RING_RADIUS, hovered == TransformGizmoHandles.Rotate ? ArrowHighlightColor : RingColor);
    }

    void DrawArrowOnRing(DrawingContext context, double angle, double radius, IBrush color)
    {
        double x = Math.Cos(angle) * radius;
        double y = Math.Sin(angle) * radius;
        var tip = new Point(x, y);
        var tangent = new Vector(-Math.Sin(angle), Math.Cos(angle));
        var baseCenter = tip - tangent * ARROW_HEAD_SIZE;
        var perpendicular = new Vector(-tangent.Y, tangent.X);
        var pt1 = baseCenter + perpendicular * (ARROW_HEAD_SIZE * 0.5);
        var pt2 = baseCenter - perpendicular * (ARROW_HEAD_SIZE * 0.5);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(tip, true);
            ctx.LineTo(pt1);
            ctx.LineTo(pt2);
            ctx.EndFigure(true);
        }
        context.DrawGeometry(color, null, geometry);
    }

    const double RING_RADIUS = 32;
    const double RING_THICKNESS = 2;
    const double HIT_THICKNESS = 6;
    const double ARROW_HEAD_SIZE = 10;
    const double ARROW_BASE_ANGLE = -Math.PI * 0.25;
    const double RAY_THICKNESS = 2;
    static readonly IBrush RingColor = new SolidColorBrush(Colors.Blue);
    static readonly IBrush ArrowHighlightColor = new SolidColorBrush(Colors.Orange);
    static readonly IBrush RayColor = new SolidColorBrush(Colors.Blue);
}