using System;
using System.Numerics;
using Avalonia.Media;

namespace Fantania;

public static class VectorExtensions
{
    public static bool IsZero(this Avalonia.Vector self)
    {
        return self.X == 0.0 && self.Y == 0.0;
    }

    public static bool IsZero(this Vector2 self)
    {
        return self.X == 0.0f && self.Y == 0.0f;
    }

    public static Vector2 ToVector2(this Avalonia.Vector self)
    {
        return new Vector2((float)self.X, (float)self.Y);
    }

    public static Vector2 ToVector2(this Avalonia.Point self)
    {
        return new Vector2((float)self.X, (float)self.Y);
    }

    public static Vector2 ToVector2(this Avalonia.Size self)
    {
        return new Vector2((float)self.Width, (float)self.Height);
    }

    public static Color ToColor(this Vector4 self)
    {
        return new Color((byte)(self.W * 255), (byte)(self.X * 255), (byte)(self.Y * 255), (byte)(self.Z * 255));
    }

    public static Vector4 ToVector4(this Color self)
    {
        return new Vector4(self.R / 255.0f, self.G / 255.0f, self.B / 255.0f, self.A / 255.0f);
    }

    public static Vector4 Gamma(this Vector4 self)
    {
        return new Vector4(
            MathF.Pow(self.X, 1.0f / 2.2f),
            MathF.Pow(self.Y, 1.0f / 2.2f),
            MathF.Pow(self.Z, 1.0f / 2.2f),
            self.W
        );
    }

    public static Vector4 CrtGamma(this Vector4 self)
    {
        return new Vector4(
            MathF.Pow(self.X, 2.2f),
            MathF.Pow(self.Y, 2.2f),
            MathF.Pow(self.Z, 2.2f),
            self.W
        );
    }

    public static Vector4 Srgb2Linear(this Vector4 self)
    {
        return new Vector4(
            Srgb2Linear(self.X),
            Srgb2Linear(self.Y),
            Srgb2Linear(self.Z),
            self.W
        );
    }

    public static Vector4 Linear2Srgb(this Vector4 self)
    {
        return new Vector4(
            Linear2Srgb(self.X),
            Linear2Srgb(self.Y),
            Linear2Srgb(self.Z),
            self.W
        );
    }

    static float Srgb2Linear(float value)
    {
        return value <= 0.04045f ? (value / 12.92f) : MathF.Pow((value + 0.055f) / 1.055f, 2.4f);
    }

    static float Linear2Srgb(float value)
    {
        return value <= 0.0031308f ? (12.92f * value) : (1.055f * MathF.Pow(value, 1.0f / 2.4f) - 0.055f);
    }

    public static float Cross(this Vector2 self, Vector2 other)
    {
        return self.X * other.Y - self.Y * other.X;
    }
}