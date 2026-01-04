using System.Numerics;
using System.Runtime.InteropServices;

namespace FantaniaLib;

/// <summary>
/// Column Major
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Matrix3x3 : IEquatable<Matrix3x3>
{
    public static readonly Matrix3x3 Identity = new Matrix3x3();

    public static Matrix3x3 CreateTranslation(Vector2 translation)
    {
        Matrix3x3 ret = Identity;
        ret.m20 = translation.X;
        ret.m21 = translation.Y;
        return ret;
    }

    public static Matrix3x3 CreateRotation(float inRadian)
    {
        float cos = MathF.Cos(inRadian);
        float sin = MathF.Sin(inRadian);
        Matrix3x3 ret = Identity;
        ret.m00 = cos;
        ret.m01 = -sin;
        ret.m10 = sin;
        ret.m11 = cos;
        return ret;
    }

    public static Matrix3x3 CreateScale(Vector2 scale)
    {
        Matrix3x3 ret = Identity;
        ret.m00 = scale.X;
        ret.m11 = scale.Y;
        return ret;
    }

    public static Matrix3x3 CreateSkew(Vector2 skew)
    {
        Matrix3x3 ret = Identity;
        ret.m10 = skew.X;
        ret.m01 = skew.Y;
        return ret;
    }

    public Matrix3x3()
    {
        m00 = m11 = m22 = 1.0f;
    }

    public Matrix3x3(float v00, float v01, float v02, float v10, float v11, float v12, float v20, float v21, float v22)
    {
        m00 = v00;
        m01 = v01;
        m02 = v02;
        m10 = v10;
        m11 = v11;
        m12 = v12;
        m20 = v20;
        m21 = v21;
        m22 = v22;
    }

    public Matrix3x3(Vector3 col0, Vector3 col1, Vector3 col2)
    : this(col0.X, col0.Y, col0.Z, col1.X, col1.Y, col1.Z, col2.X, col2.Y, col2.Z)
    { }

    public Vector3 GetRow(int row)
    {
        if (row == 0)
            return new Vector3(m00, m10, m20);
        else if (row == 1)
            return new Vector3(m01, m11, m21);
        else if (row == 2)
            return new Vector3(m02, m12, m22);
        throw new System.ArgumentOutOfRangeException("row");
    }

    public Vector3 GetColumn(int column)
    {
        if (column == 0)
            return new Vector3(m00, m01, m02);
        else if (column == 1)
            return new Vector3(m10, m11, m12);
        else if (column == 2)
            return new Vector3(m20, m21, m22);
        throw new System.ArgumentOutOfRangeException("column");
    }

    public bool Equals(Matrix3x3 other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is not Matrix3x3) return false;
        return Equals((Matrix3x3)obj);
    }

    public override int GetHashCode()
    {
        int hashCode = (m00.GetHashCode() * 397) ^ m01.GetHashCode();
        hashCode = (hashCode * 397) ^ m02.GetHashCode();
        hashCode = (hashCode * 397) ^ m10.GetHashCode();
        hashCode = (hashCode * 397) ^ m11.GetHashCode();
        hashCode = (hashCode * 397) ^ m12.GetHashCode();
        hashCode = (hashCode * 397) ^ m20.GetHashCode();
        hashCode = (hashCode * 397) ^ m21.GetHashCode();
        hashCode = (hashCode * 397) ^ m22.GetHashCode();
        return hashCode;
    }

    public static Vector2 operator *(Matrix3x3 mat, Vector2 vec)
    {
        Vector3 homo = mat * new Vector3(vec, 1.0f);
        return new Vector2(homo.X, homo.Y);
    }

    public static Vector3 operator *(Matrix3x3 mat, Vector3 vec)
    {
        float x = Vector3.Dot(mat.GetRow(0), vec);
        float y = Vector3.Dot(mat.GetRow(1), vec);
        float z = Vector3.Dot(mat.GetRow(2), vec);
        return new Vector3(x, y, z);
    }

    public static Matrix3x3 operator *(Matrix3x3 mat1, Matrix3x3 mat2)
    {
        float m00 = Vector3.Dot(mat1.GetRow(0), mat2.GetColumn(0));
        float m01 = Vector3.Dot(mat1.GetRow(1), mat2.GetColumn(0));
        float m02 = Vector3.Dot(mat1.GetRow(2), mat2.GetColumn(0));
        float m10 = Vector3.Dot(mat1.GetRow(0), mat2.GetColumn(1));
        float m11 = Vector3.Dot(mat1.GetRow(1), mat2.GetColumn(1));
        float m12 = Vector3.Dot(mat1.GetRow(2), mat2.GetColumn(1));
        float m20 = Vector3.Dot(mat1.GetRow(0), mat2.GetColumn(2));
        float m21 = Vector3.Dot(mat1.GetRow(1), mat2.GetColumn(2));
        float m22 = Vector3.Dot(mat1.GetRow(2), mat2.GetColumn(2));
        return new Matrix3x3(m00, m01, m02, m10, m11, m12, m20, m21, m22);
    }

    public static bool operator ==(Matrix3x3 mat1, Matrix3x3 mat2)
    {
        return mat1.m00 == mat2.m00 && mat1.m01 == mat2.m01 && mat1.m02 == mat2.m02 &&
            mat1.m10 == mat2.m10 && mat1.m11 == mat2.m11 && mat1.m12 == mat2.m12 &&
            mat1.m20 == mat2.m20 && mat1.m21 == mat2.m21 && mat1.m22 == mat2.m22;
    }

    public static bool operator !=(Matrix3x3 mat1, Matrix3x3 mat2)
    {
        return !(mat1 == mat2);
    }

    public override string ToString()
    {
        return $"[{GetRow(0)}, {GetRow(1)}, {GetRow(2)}]";
    }

    public float m00;      // Column 0 Row 0
    public float m01;      // Column 0 Row 1
    public float m02;      // Column 0 Row 2
    public float m10;      // Column 1 Row 0
    public float m11;      // Column 1 Row 1
    public float m12;      // Column 1 Row 2
    public float m20;      // Column 2 Row 0
    public float m21;      // Column 2 Row 1
    public float m22;      // Column 2 Row 2
}