using System;
using System.Collections.Generic;
using System.Linq;

namespace Fantania;

public struct Distribution<T>
{
    public float probability;
    public Func<T[], int, T> valueFunction; // Parameters: 已生成的值,当前下标
}

public class SegmentedProbabilityDistribution<T> : IProbabilityDistribution<T>
{
    public SegmentedProbabilityDistribution()
    {
    }

    public void AddSegmentedDistribution(float probability, Func<T[], int, T> func)
    {
        AddSegmentedDistribution(new Distribution<T>
        {
            probability = probability,
            valueFunction = func,
        });
    }

    public void AddSegmentedDistribution(Distribution<T> distribution)
    {
        _distributions.Add(distribution);
        _dirty = true;
    }

    public void Clear()
    {
        _distributions.Clear();
    }

    public T GetValue(float probability, T[] values, int currentIdx)
    {
        if (_dirty)
            Build();
        foreach (var pair in _builded)
        {
            if (probability < pair.Key)
                return pair.Value.Invoke(values, currentIdx);
        }
        throw new ArgumentOutOfRangeException("probability");
    }

    void Build()
    {
        _builded = new SortedDictionary<float, Func<T[], int, T>>();
        float sum = _distributions.Sum(dis => dis.probability);
        float acc = 0.0f;
        for (int i = 0; i < _distributions.Count; i++)
        {
            acc += _distributions[i].probability;
            _builded[acc / sum] = _distributions[i].valueFunction;
        }
    }

    List<Distribution<T>> _distributions = new List<Distribution<T>>(4);
    SortedDictionary<float, Func<T[], int, T>> _builded;
    bool _dirty = true;
}