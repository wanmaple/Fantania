using System.Numerics;

namespace FantaniaLib;

public class Camera2D
{
    public float MinZoom { get; private set; }
    public float MaxZoom { get; private set; }
    public Vector2Int Viewport { get; private set; }

    private Vector2 _pos = Vector2.Zero;
    public Vector2 Position
    {
        get { return _pos; }
        set
        {
            if (_pos != value)
            {
                _pos = value;
                _dirty = true;
            }
        }
    }

    private float _zoom = 1.0f;
    public float Zoom
    {
        get { return _zoom; }
        set
        {
            if (_zoom != value)
            {
                _zoom = value;
                _dirty = true;
            }
        }
    }

    public Matrix3x3 ViewMatrix
    {
        get
        {
            if (_dirty)
                UpdateViewMatrix();
            return _matView;
        }
    }

    public Camera2D(Vector2Int viewport, float minZoom = 0.1f, float maxZoom = 10.0f)
    {
        MinZoom = MathF.Max(minZoom, 0.1f);
        MaxZoom = MathF.Min(maxZoom, 10.0f);
        Viewport = viewport;
    }

    public void Translate(Vector2 translation)
    {
        Position += translation;
    }

    public void ZoomAt(float amount, Vector2 centerInWorld)
    {
        float oldZoom = _zoom;
        Zoom = MathHelper.Clamp(Zoom + amount, MinZoom, MaxZoom);
        float zoomFactor = Zoom / oldZoom;
        Vector2 offset = (centerInWorld - Position) * (1.0f - 1.0f / zoomFactor);
        Position += offset;
    }

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return screenPos / Zoom + Position;
    }

    void UpdateViewMatrix()
    {
        _matView = Matrix3x3.CreateScale(new Vector2(Zoom, Zoom)) * Matrix3x3.CreateTranslation(-Position);
    }

    bool _dirty = true;
    Matrix3x3 _matView = Matrix3x3.Identity;
}