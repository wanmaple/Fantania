using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public static class VectorExtensions
{
    public static Color ToColor(this Vector4 self)
    {
        return new Color((byte)(self.W * 255), (byte)(self.X * 255), (byte)(self.Y * 255), (byte)(self.Z * 255));
    }

    public static Vector4 ToVector4(this Color self)
    {
        return new Vector4(self.R / 255.0f, self.G / 255.0f, self.B / 255.0f, self.A / 255.0f);
    }

    public static string ToHex(this Color self)
    {
        return $"#{self.A:X2}{self.R:X2}{self.G:X2}{self.B:X2}";
    }
}