namespace FantaniaLib;

public class ReferenceCounter<T>
{
    public T Item { get; private set; }

    public int Referenced => _count;
    public bool IsFree => _count <= 0;

    public ReferenceCounter(T item)
    {
        Item = item;
    }

    public void Acquire()
    {
        _count++;
    }

    public void Release()
    {
        _count--;
    }

    int _count = 1;
}