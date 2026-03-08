using System.Collections;
using System.Text;

namespace FantaniaLib;

public struct FantaniaArray<T> : IList<T>, IReadOnlyList<T>, ICloneable where T : notnull
{
    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => _items[index];
        set
        {
            if (!_items[index]!.Equals(value))
            {
                _items[index] = value;
            }
        }
    }

    public FantaniaArray()
    {
    }

    public void Add(T item)
    {
        _items.Add(item);
    }

    public void Insert(int index, T item)
    {
        _items.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
    }

    public bool Remove(T item)
    {
        bool result = _items.Remove(item);
        return result;
    }

    public int IndexOf(T item)
    {
        return _items.IndexOf(item);
    }

    public bool Contains(T item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static bool operator==(FantaniaArray<T> a, FantaniaArray<T> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (!a[i].Equals(b[i]))
                return false;
        }
        return true;
    }

    public static bool operator!=(FantaniaArray<T> a, FantaniaArray<T> b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        return obj is FantaniaArray<T> other && this == other;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (var item in _items)
        {
            hash = HashCode.Combine(hash, item);
        }
        return hash;
    }

    public FantaniaArray<T> Clone()
    {
        var clone = new FantaniaArray<T>();
        foreach (var item in _items)
        {
            clone.Add(item);
        }
        return clone;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    List<T> _items = new List<T>(8);
}