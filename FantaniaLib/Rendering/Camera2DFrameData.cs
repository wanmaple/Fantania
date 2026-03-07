using System.Numerics;

namespace FantaniaLib;

public class Camera2DFrameData
{
    public Vector2 Position { get; private set; }
    public float Zoom { get; private set; }

    public Camera2DFrameData(Camera2D camera)
    {
        Position = camera.Position;
        Zoom = camera.Zoom;
    }

    public Vector2 ScreenPositionToWorldPosition(Vector2 screenPos)
    {
        return screenPos / Zoom + Position;
    }

    public Vector2 WorldPositionToScreenPosition(Vector2 worldPos)
    {
        return (worldPos - Position) * Zoom;
    }

    public Vector2 ScreenMovementToWorldMovement(Vector2 screenVec)
    {
        return screenVec / Zoom;
    }

    public Vector2 WorldMovementToScreenMovement(Vector2 worldVec)
    {
        return worldVec * Zoom;
    }
}
