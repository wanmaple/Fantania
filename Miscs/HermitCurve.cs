using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Fantania;

public struct HermitPivot : IEquatable<HermitPivot>
{
    public const float MIN_TANGENT = -11.4f;
    public const float MAX_TANGENT = 11.4f;

    /// <summary>
    /// Normalized value, between [0, 1].
    /// </summary>
    public float Value { get; set; }
    /// <summary>
    /// Slope value actually, between [-11.4, 11.4], around tan(85).
    /// </summary>
    public float Tangent { get; set; }
    public int Index { get; set; }

    public bool Equals(HermitPivot other)
    {
        return this == other;
    }

    public override bool Equals(object obj)
    {
        return obj is HermitPivot && Equals((HermitPivot)obj);
    }

    public override int GetHashCode()
    {
        int hash = Value.GetHashCode();
        hash = (hash * 397) ^ Tangent.GetHashCode();
        hash = (hash * 397) ^ Index;
        return hash;
    }

    public static bool operator ==(HermitPivot lhs, HermitPivot rhs)
    {
        return lhs.Value == rhs.Value && lhs.Tangent == rhs.Tangent && lhs.Index == rhs.Index;
    }

    public static bool operator !=(HermitPivot lhs, HermitPivot rhs)
    {
        return !(lhs == rhs);
    }
}

public struct HermitCoefficients
{
    public float a0, a1, a2, a3;

    public float Evaluate(float t)
    {
        return a0 + a1 * t + a2 * t * t + a3 * t * t * t;
    }
}

public struct HermitCurve : IEquatable<HermitCurve>
{
    public const int SEGMENTS = 16;

    public static readonly HermitCurve StraightLineAt1 = new HermitCurve();
    public static readonly HermitCurve DiagonalLineDownward = new HermitCurve(1.0f, -1.0f, 0.0f, -1.0f);
    public static readonly HermitCurve DiagonalLineUpward = new HermitCurve(0.0f, 1.0f, 1.0f, 1.0f);

    public event Action CurveChanged;

    public HermitPivot StartPoint => _pivots[0].Value;
    public HermitPivot EndPoint => _pivots[SEGMENTS - 1].Value;
    public IEnumerable<HermitPivot> Pivots
    {
        get
        {
            for (int i = 0; i < SEGMENTS; i++)
            {
                if (_pivots[i] != null)
                    yield return _pivots[i].Value;
            }
        }
    }

    public HermitCurve()
    : this(1.0f, 0.0f, 1.0f, 0.0f)
    {
    }

    public HermitCurve(float val0, float tan0, float val1, float tan1)
    {
        _pivots[0] = new HermitPivot { Value = val0, Tangent = tan0, Index = 0, };
        _pivots[SEGMENTS - 1] = new HermitPivot { Value = val1, Tangent = tan1, Index = SEGMENTS - 1, };
        _coeffs[0] = CalculateCoefficient(_pivots[0].Value, _pivots[SEGMENTS - 1].Value);
    }

    public bool TryGetPivot(int seg, out HermitPivot pivot)
    {
        pivot = default;
        if (_pivots[seg] != null)
        {
            pivot = _pivots[seg].Value;
            return true;
        }
        return false;
    }

    public void AddPivot(int seg, float value, float tangent)
    {
        if (_pivots[seg] != null)
            throw new ArgumentException($"Pivot at {seg} was already added.");
        var pivot = new HermitPivot { Value = value, Tangent = tangent, Index = seg, };
        _pivots[seg] = pivot;
        HermitPivot prev = FindPreviousPivot(seg - 1);
        HermitPivot next = FindNextPivot(seg);
        _coeffs[prev.Index] = CalculateCoefficient(prev, pivot);
        _coeffs[seg] = CalculateCoefficient(pivot, next);
        CurveChanged?.Invoke();
    }

    public void RemovePivot(int seg)
    {
        if (_pivots[seg] == null)
            throw new ArgumentException($"Pivot at {seg} doesn't exist.");
        _pivots[seg] = null;
        HermitPivot prev = FindPreviousPivot(seg - 1);
        HermitPivot next = FindNextPivot(seg);
        _coeffs[prev.Index] = CalculateCoefficient(prev, next);
        _coeffs[seg] = null;
        CurveChanged?.Invoke();
    }

    public void UpdatePivot(int seg, float value, float tangent)
    {
        if (_pivots[seg] == null)
            throw new ArgumentException($"Pivot at {seg} doesn't exist.");
        var pivot = new HermitPivot { Value = value, Tangent = tangent, Index = seg, };
        _pivots[seg] = pivot;
        if (seg == 0)
        {
            HermitPivot next = FindNextPivot(seg);
            _coeffs[seg] = CalculateCoefficient(pivot, next);
        }
        else if (seg == SEGMENTS - 1)
        {
            HermitPivot prev = FindPreviousPivot(seg - 1);
            _coeffs[prev.Index] = CalculateCoefficient(prev, pivot);
        }
        else
        {
            HermitPivot prev = FindPreviousPivot(seg - 1);
            HermitPivot next = FindNextPivot(seg);
            _coeffs[prev.Index] = CalculateCoefficient(prev, pivot);
            _coeffs[seg] = CalculateCoefficient(pivot, next);
        }
        CurveChanged?.Invoke();
    }

    public float Evaluate(float t, EvaluateModes mode = EvaluateModes.Clamp)
    {
        if (mode == EvaluateModes.Clamp)
            t = MathHelper.Clamp(t, 0.0f, 1.0f);
        else
        {
            if (t > 1.0f)
            {
                t = t - MathHelper.FloorToInt(t);
            }
            else if (t < 0.0f)
            {
                t = t + MathHelper.CeilToInt(-t);
            }
        }
        int seg = (int)(t * (SEGMENTS - 1));
        if (seg == 0)
        {
            HermitPivot next = FindNextPivot(seg);
            t = t * (SEGMENTS - 1) / next.Index;
            return _coeffs[0].Value.Evaluate(t);
        }
        else if (seg == SEGMENTS - 1)
        {
            HermitPivot prev = FindPreviousPivot(seg);
            return _coeffs[prev.Index].Value.Evaluate(1.0f);
        }
        else
        {
            HermitPivot prev = FindPreviousPivot(seg);
            HermitPivot next = FindNextPivot(seg);
            if (prev.Index != 0)
            {

            }
            float range = next.Index - prev.Index;
            t = (t * (SEGMENTS - 1) - prev.Index) / range;
            return _coeffs[prev.Index].Value.Evaluate(t);
        }
    }

    public HermitCurve Clone()
    {
        HermitCurve copy = new HermitCurve();
        for (int i = 0; i < SEGMENTS; i++)
        {
            copy._pivots[i] = _pivots[i];
        }
        for (int i = 0; i < _coeffs.Length; i++)
        {
            copy._coeffs[i] = _coeffs[i];
        }
        return copy;
    }

    public bool Equals(HermitCurve other)
    {
        return this == other;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is HermitCurve && Equals((HermitCurve)obj);
    }

    public override int GetHashCode()
    {
        int hash = 0;
        for (int i = 0; i < SEGMENTS; i++)
        {
            HermitPivot? pivot = _pivots[i];
            hash = (hash * 397) ^ pivot.GetHashCode();
        }
        return hash;
    }

    public static bool operator ==(HermitCurve lhs, HermitCurve rhs)
    {
        for (int i = 0; i < SEGMENTS; i++)
        {
            HermitPivot? lp = lhs._pivots[i];
            HermitPivot? rp = rhs._pivots[i];
            if (lp != rp)
                return false;
        }
        return true;
    }

    public static bool operator !=(HermitCurve lhs, HermitCurve rhs)
    {
        return !(lhs == rhs);
    }

    HermitPivot FindPreviousPivot(int seg)
    {
        if (seg == SEGMENTS - 1)
            seg--;
        for (int i = seg; i >= 0; i--)
            {
                if (_pivots[i] != null)
                    return _pivots[i].Value;
            }
        throw new ArgumentException($"Cannot find previous pivot at {seg}.");
    }

    HermitPivot FindNextPivot(int seg)
    {
        for (int i = seg + 1; i < SEGMENTS; i++)
        {
            if (_pivots[i] != null)
                return _pivots[i].Value;
        }
        throw new ArgumentException($"Cannot find next pivot at {seg}.");
    }

    HermitCoefficients CalculateCoefficient(HermitPivot pivot1, HermitPivot pivot2)
    {
        float x1 = 0.0f;
        float x2 = 1.0f;
        Matrix4x4 mat = new Matrix4x4(
            1.0f, 0.0f, 1.0f, 0.0f,
            x1, 1.0f, x2, 1.0f,
            P2(x1), 2.0f * x1, P2(x2), 2.0f * x2,
            P3(x1), 3.0f * P2(x1), P3(x2), 3.0f * P2(x2)
        );
        Vector4 vec = new Vector4(pivot1.Value, pivot1.Tangent, pivot2.Value, pivot2.Tangent);
        Matrix4x4.Invert(mat, out var invMat);
        Vector4 coeff = Vector4.Transform(vec, invMat);
        return new HermitCoefficients
        {
            a0 = coeff.X,
            a1 = coeff.Y,
            a2 = coeff.Z,
            a3 = coeff.W,
        };
    }

    float P2(float val)
    {
        return val * val;
    }

    float P3(float val)
    {
        return val * val * val;
    }

    HermitPivot?[] _pivots = new HermitPivot?[SEGMENTS];
    HermitCoefficients?[] _coeffs = new HermitCoefficients?[SEGMENTS - 1];
}