using System.Collections.Generic;
using System.Numerics;

namespace Fantania;

public interface IRandomNumberGenerator
{
    float Next();
    float NextRange(float min, float max);
    int NextInteger();
    int NextIntegerRange(int min, int max);
    Vector2 RandomUnitVector();
    IEnumerable<T> Select<T>(int num, IEnumerable<T> samples);
}