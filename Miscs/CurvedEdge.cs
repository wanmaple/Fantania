using System;

namespace Fantania;

public struct CurvedEdge : IEquatable<CurvedEdge>
{
    public static readonly CurvedEdge Flat = new CurvedEdge();

    [EditCurve]
    public HermitCurve Curve { get; set; }

    [EditInteger(0)]
    public int CurveAmplitude { get; set; }

    [EditInteger(ControlType = typeof(RandomSeedControl))]
    public int NoiseSeed { get; set; }
    
    [EditDecimal(0.0, 0.5, ControlType = typeof(SliderDecimalControl))]
    public double NoiseAmplitude { get; set; }

    public CurvedEdge()
    {
        Curve = HermitCurve.StraightLineAt1;
        CurveAmplitude = 0;
        NoiseSeed = 0;
        NoiseAmplitude = 0.0;
    }

    public CurvedEdge Clone()
    {
        return new CurvedEdge
        {
            Curve = this.Curve.Clone(),
            CurveAmplitude = this.CurveAmplitude,
            NoiseSeed = this.NoiseSeed,
            NoiseAmplitude = this.NoiseAmplitude,
        };
    }

    public static bool operator ==(CurvedEdge lhs, CurvedEdge rhs)
    {
        return lhs.Curve == rhs.Curve && lhs.CurveAmplitude == rhs.CurveAmplitude && lhs.NoiseSeed == rhs.NoiseSeed && lhs.NoiseAmplitude == rhs.NoiseAmplitude;
    }

    public static bool operator !=(CurvedEdge lhs, CurvedEdge rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(CurvedEdge other)
    {
        return this == other;
    }

    public override bool Equals(object obj)
    {
        return obj is CurvedEdge && Equals((CurvedEdge)obj);
    }

    public override int GetHashCode()
    {
        int hash = Curve.GetHashCode();
        hash = (hash * 397) ^ CurveAmplitude.GetHashCode();
        hash = (hash * 397) ^ NoiseSeed;
        hash = (hash * 397) ^ NoiseAmplitude.GetHashCode();
        return hash;
    }
}