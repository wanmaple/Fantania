using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Fantania;

public class SystemRNG : IRandomNumberGenerator
{
    public SystemRNG()
    : this(DateTime.Now.GetHashCode())
    { }

    public SystemRNG(int seed)
    {
        _random = new Random(seed);
    }

    public float Next()
    {
        return _random.NextSingle();
    }

    public int NextInteger()
    {
        return _random.Next();
    }

    public int NextIntegerRange(int min, int max)
    {
        return _random.Next(min, max);
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

    Random _random;
}
