namespace Fantania;

public interface IProbabilityDistribution<T>
{
    public T GetValue(float probability, T[] generated, int currentIdx);
}