namespace FantaniaLib;

public interface INoise2D
{
    int Seed { get; set; }

    void TransformCoordinate(ref float x, ref float y);
    /// <summary>
    /// 获取指定坐标的噪声值，返回值范围为[-1,1]
    /// </summary>
    float Noise(float x, float y, int repeat);
}