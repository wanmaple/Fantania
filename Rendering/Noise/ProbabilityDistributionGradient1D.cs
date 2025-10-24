namespace Fantania;

public class ProbabilityDistributionGradient1D : UniformGradient1D
{
    public int RandomSeed { get; private set; }

    public ProbabilityDistributionGradient1D(float segment, int maxSegments, IProbabilityDistribution<float> distribution, IRandomNumberGenerator? gen)
    : base(segment, maxSegments)
    {
        if (gen == null)
            gen = new SystemRNG();
        _rng = gen;
        _distribution = distribution;
        Build();
    }

    void Build()
    {
        for (int i = 0; i < MaxSegments; i++)
        {
            float prob = _rng.Next();
            float value = _distribution.GetValue(prob, Values, i);
            Values[i] = value;
        }
    }

    IRandomNumberGenerator _rng;
    IProbabilityDistribution<float> _distribution;
}