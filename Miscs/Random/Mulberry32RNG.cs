using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Fantania;

public class Mulberry32RNG : IRandomNumberGenerator
{
    public Mulberry32RNG()
    : this(DateTime.Now.GetHashCode())
    { }

    public Mulberry32RNG(int seed)
    {
        unchecked
        {
            uint toUnsigned = (uint)seed;
            _seed = toUnsigned;
        }
    }

    public float Next()
    {
        return (uint)NextInteger() / (float)uint.MaxValue;
    }

    public int NextInteger()
    {
        _seed += 0x6D2B79F5;
        uint t = (_seed ^ (_seed >> 15)) * (1 | _seed);
        t = (t + ((t ^ (t >> 7)) * (61 | t))) ^ t;
        unchecked
        {
            return (int)(t ^ (t >> 14));
        }
    }

    public int NextIntegerRange(int min, int max)
    {
        return (int)NextRange(min, max);
    }

    public float NextRange(float min, float max)
    {
        return Next() * (max - min) + min;
    }

    public Vector2 RandomUnitVector()
    {
        float angle = NextRange(0.0f, MathF.PI * 2.0f);
        return new Vector2(MathF.Cos(angle), -MathF.Sin(angle));
    }

    public IEnumerable<T> Select<T>(int num, IEnumerable<T> samples)
    {
        int remain = num;
        int rest = samples.Count();
        for (int i = 0; i < samples.Count(); i++)
        {
            float prob = (float)remain / rest;
            if (prob >= 1.0f || Next() <= prob)
            {
                remain--;
                yield return samples.ElementAt(i);
            }
            rest--;
            if (rest <= 0)
                break;
        }
    }

    uint _seed;
}
