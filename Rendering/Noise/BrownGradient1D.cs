using System;

namespace Fantania;

public class BrownGradient1D : UniformGradient1D
{
    public BrownGradient1D(float segment, int maxSegments, float r = 0.5f, IRandomNumberGenerator? gen = null)
    : base(segment, maxSegments)
    {
        if (gen == null)
            gen = new SystemRNG();
        _rng = gen;
        _r = r;
        Build();
    }

    void Build()
    {
        Values[0] = _rng.NextRange(-1.0f, 1.0f);
        for (int i = 1; i < MaxSegments; i++)
        {
            Values[i] = Values[i - 1] * _r + MathF.Sqrt(1.0f - _r * _r) * _rng.NextRange(-1.0f, 1.0f);
        }
    }

    IRandomNumberGenerator _rng;
    float _r;
}
