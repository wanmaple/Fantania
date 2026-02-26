namespace FantaniaLib;

public class LevelEditConfig
{
    public int GridAlign { get; set; } = 16;
    public float ZoomSensitivity { get; set; } = 1.5f;
    public float ZoomMin { get; set; } = 0.1f;
    public float ZoomMax { get; set; } = 10.0f;

    public Dictionary<int, string> LayerNames { get; set; } = new Dictionary<int, string>();
}