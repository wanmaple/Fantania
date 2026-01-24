using System.Numerics;

namespace FantaniaLib;

public class UserTemporary
{
    public string LatestEditingLevel { get; set; } = string.Empty;
    public Vector2 CameraPosition { get; set; } = Vector2.Zero;
    public float CameraZoom { get; set; } = 1.0f;
}