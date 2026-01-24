using System.Numerics;

namespace FantaniaLib;

public static class MathHelper
{
    public static byte ToByte(float val)
    {
        return (byte)(val * 255);
    }

    public static int FloorToInt(float num)
    {
        return (int)MathF.Floor(num);
    }

    public static int CeilToInt(float num)
    {
        return (int)MathF.Ceiling(num);
    }

    public static int RoundToInt(float num)
    {
        return (int)MathF.Round(num);
    }

    public static int Clamp(int num, int min, int max)
    {
        return Math.Max(min, Math.Min(max, num));
    }

    public static float Clamp(float num, float min, float max)
    {
        return Math.Max(min, Math.Min(max, num));
    }

    public static Vector2 Clamp(Vector2 vec, Vector2 min, Vector2 max)
    {
        return new Vector2(Clamp(vec.X, min.X, max.X), Clamp(vec.Y, min.Y, max.Y));
    }

    public static Vector3 Clamp(Vector3 vec, Vector3 min, Vector3 max)
    {
        return new Vector3(Clamp(vec.X, min.X, max.X), Clamp(vec.Y, min.Y, max.Y), Clamp(vec.Z, min.Z, max.Z));
    }

    public static Vector4 Clamp(Vector4 vec, Vector4 min, Vector4 max)
    {
        return new Vector4(Clamp(vec.X, min.X, max.X), Clamp(vec.Y, min.Y, max.Y), Clamp(vec.Z, min.Z, max.Z), Clamp(vec.W, min.W, max.W));
    }

    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * t;
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t), Lerp(a.Z, b.Z, t));
    }

    public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
    {
        return new Vector4(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t), Lerp(a.Z, b.Z, t), Lerp(a.W, b.W, t));
    }

    public static float CubicLerp(float a, float b, float c, float d, float t)
    {
        float p = (d - c) - (a - b);
        return t * t * t * p + t * t * ((a - b) - p) + t * (c - a) + b;
    }

    public static Vector2 CubicLerp(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        return new Vector2(CubicLerp(a.X, b.X, c.X, d.X, t), CubicLerp(a.Y, b.Y, c.Y, d.Y, t));
    }

    public static Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return new Vector3(CubicLerp(a.X, b.X, c.X, d.X, t), CubicLerp(a.Y, b.Y, c.Y, d.Y, t), CubicLerp(a.Z, b.Z, c.Z, d.Z, t));
    }

    public static Vector4 CubicLerp(Vector4 a, Vector4 b, Vector4 c, Vector4 d, float t)
    {
        return new Vector4(CubicLerp(a.X, b.X, c.X, d.X, t), CubicLerp(a.Y, b.Y, c.Y, d.Y, t), CubicLerp(a.Z, b.Z, c.Z, d.Z, t), CubicLerp(a.W, b.W, c.W, d.W, t));
    }

    public static float Hermite(float t)
    {
        return t * t * (3.0f - 2.0f * t);
    }

    public static Vector2 Hermite(Vector2 t)
    {
        return new Vector2(Hermite(t.X), Hermite(t.Y));
    }

    public static Vector3 Hermite(Vector3 t)
    {
        return new Vector3(Hermite(t.X), Hermite(t.Y), Hermite(t.Z));
    }

    public static Vector4 Hermite(Vector4 t)
    {
        return new Vector4(Hermite(t.X), Hermite(t.Y), Hermite(t.Z), Hermite(t.W));
    }

    public static float Quintic(float t)
    {
        return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
    }

    public static Vector2 Quintic(Vector2 t)
    {
        return new Vector2(Quintic(t.X), Quintic(t.Y));
    }

    public static Vector3 Quintic(Vector3 t)
    {
        return new Vector3(Quintic(t.X), Quintic(t.Y), Quintic(t.Z));
    }

    public static Vector4 Quintic(Vector4 t)
    {
        return new Vector4(Quintic(t.X), Quintic(t.Y), Quintic(t.Z), Quintic(t.W));
    }

    public static bool IsPointInsideConvexQuadrilateral(Vector2 pt, IReadOnlyList<Vector2> verts)
    {
        Vector2 v1 = verts[0] - pt;
        Vector2 v2 = verts[1] - pt;
        Vector2 v3 = verts[2] - pt;
        Vector2 v4 = verts[3] - pt;
        // 判断四个向量的叉积值为同一符号且不为0
        float c1 = v1.Cross(v2);
        float c2 = v2.Cross(v3);
        float c3 = v3.Cross(v4);
        float c4 = v4.Cross(v1);
        int s1 = MathF.Sign(c1);
        int s2 = MathF.Sign(c2);
        int s3 = MathF.Sign(c3);
        int s4 = MathF.Sign(c4);
        return s1 == s2 && s1 == s3 && s1 == s4 && s1 != 0;
    }

    public static bool IsPointInsidePolygon(Vector2 pt, IReadOnlyList<Vector2> verts)
    {
        bool result = false;
        for (int i = 0; i < verts.Count; i++)
        {
            Vector2 pt1 = verts[i];
            Vector2 pt2 = verts[(i + 1) % verts.Count];
            if (pt1.Y == pt2.Y) continue;
            Vector2 ptMin = pt1.Y < pt2.Y ? pt1 : pt2;
            Vector2 ptMax = pt1.Y < pt2.Y ? pt2 : pt1;
            if (pt.Y <= ptMin.Y || pt.Y > ptMax.Y) continue;
            Vector2 vec1 = pt - ptMin;
            Vector2 vec2 = ptMax - ptMin;
            int sign = MathF.Sign(vec1.Cross(vec2));
            if (sign < 0)
                result = !result;
        }
        return result;
    }

    public static Matrix3x3 BuildTransform(Vector2 anchor, Vector2 size, Vector2 pos, float rot, Vector2 scale)
    {
        // Transform矩阵可以分解成四部分，首先进行Anchor相关的平移，然后进行缩放，然后进行旋转，最后进行世界位置的平移。
        Matrix3x3 mat = Matrix3x3.Identity;
        if (anchor != Vector2.Zero)
        {
            mat = Matrix3x3.CreateTranslation(new Vector2(-anchor.X * size.X, -anchor.Y * size.Y));
        }
        if (scale != Vector2.One)
        {
            mat = Matrix3x3.CreateScale(scale) * mat;
        }
        if (rot != 0.0f)
        {
            mat = Matrix3x3.CreateRotation(rot) * mat;
        }
        if (pos != Vector2.Zero)
        {
            mat = Matrix3x3.CreateTranslation(pos) * mat;
        }
        return mat;
    }

    public static Rectf BuildBound(Matrix3x3 transform, Vector2 size)
    {
        Vector2 pt1 = transform * Vector2.Zero;
        Vector2 pt2 = transform * new Vector2(size.X, 0.0f);
        Vector2 pt3 = transform * size;
        Vector2 pt4 = transform * new Vector2(0.0f, size.Y);
        float minX = MathF.Min(pt1.X, MathF.Min(pt2.X, MathF.Min(pt3.X, pt4.X)));
        float maxX = MathF.Max(pt1.X, MathF.Max(pt2.X, MathF.Max(pt3.X, pt4.X)));
        float minY = MathF.Min(pt1.Y, MathF.Min(pt2.Y, MathF.Min(pt3.Y, pt4.Y)));
        float maxY = MathF.Max(pt1.Y, MathF.Max(pt2.Y, MathF.Max(pt3.Y, pt4.Y)));
        return new Rectf(minX, minY, maxX - minX, maxY - minY);
    }
}