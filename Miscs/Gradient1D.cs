using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fantania;

public struct Gradient1D : IEquatable<Gradient1D>
{
    public const int SEGMENTS = 16;

    public static readonly Gradient1D BlackWhite = new Gradient1D();
    public static readonly Gradient1D WhiteBlack = new Gradient1D(Vector4.One, new Vector4(Vector3.Zero, 1.0f));

    public IEnumerable<(int, Vector4)> Stops
    {
        get
        {
            for (int i = 0; i < SEGMENTS; i++)
            {
                if (_stops[i] != null)
                    yield return (i, _stops[i].Value);
            }
        }
    }

    public Gradient1D() : this(new Vector4(Vector3.Zero, 1.0f), Vector4.One)
    {
    }

    public Gradient1D(Vector4 startColor, Vector4 endColor)
    {
        _stops[0] = startColor;
        _stops[SEGMENTS - 1] = endColor;
    }

    public Vector4 Evaluate(float t, EvaluateModes mode = EvaluateModes.Clamp)
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
        t = t * (SEGMENTS - 1);
        int idx = MathHelper.FloorToInt(t);
        if (idx == SEGMENTS - 1)
            return _stops[SEGMENTS - 1].Value;
        int prev = FindPrevious(idx);
        int next = FindNext(idx + 1);
        t = (float)(t - prev) / (next - prev);
        return MathHelper.Lerp(_stops[prev].Value, _stops[next].Value, t);
    }

    public bool HasStop(int index)
    {
        return _stops[index] != null;
    }

    public Vector4 ColorAt(int index)
    {
        if (_stops[index] == null)
            throw new ArgumentException($"Color at {index} doesn't exist.");
        return _stops[index].Value;
    }

    public void InsertColor(int index, Vector4 color)
    {
        if (_stops[index] != null)
            throw new ArgumentException($"Color at {index} already exists.");
        _stops[index] = color;
    }

    public void EraseColor(int index)
    {
        if (_stops[index] == null)
            throw new ArgumentException($"Color at {index} doesn't exist.");
        _stops[index] = null;
    }

    public void UpdateColor(int index, Vector4 color)
    {
        if (_stops[index] == null)
            throw new ArgumentException($"Color at {index} doesn't exist.");
        _stops[index] = color;
    }

    public Gradient1D Clone()
    {
        var clone = new Gradient1D();
        for (int i = 0; i < _stops.Length; i++)
        {
            clone._stops[i] = _stops[i];
        }
        return clone;
    }

    public bool Equals(Gradient1D other)
    {
        return this == other;
    }

    public override bool Equals(object obj)
    {
        return obj is Gradient1D && Equals((Gradient1D)obj);
    }

    public override int GetHashCode()
    {
        int hash = 0;
        for (int i = 0; i < _stops.Length; i++)
        {
            hash = (hash * 397) ^ _stops[i].GetHashCode();
        }
        return hash;
    }

    public static bool operator ==(Gradient1D lhs, Gradient1D rhs)
    {
        for (int i = 0; i < lhs._stops.Length; i++)
        {
            if (lhs._stops[i] != rhs._stops[i])
                return false;
        }
        return true;
    }

    public static bool operator !=(Gradient1D lhs, Gradient1D rhs)
    {
        return !(lhs == rhs);
    }

    int FindPrevious(int index)
    {
        for (int i = index; i >= 0; i--)
        {
            if (_stops[i] != null)
                return i;
        }
        throw new Exception();
    }
    int FindNext(int index)
    {
        for (int i = index; i < _stops.Length; i++)
        {
            if (_stops[i] != null)
                return i;
        }
        throw new Exception();
    }

    Vector4?[] _stops = new Vector4?[SEGMENTS];
}