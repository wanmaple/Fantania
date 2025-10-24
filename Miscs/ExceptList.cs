
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fantania;

public class ExceptList<T>
{
    public ExceptList(IEnumerable<T> items)
    {
        _items = new T[items.Count()];
        int idx = 0;
        foreach (var item in items)
        {
            _items[idx] = item;
            idx++;
        }
    }

    public IEnumerable<T> Except(ISet<T> excepts)
    {
        foreach (var item in _items)
        {
            if (excepts.Contains(item))
                continue;
            yield return item;
        }
    }

    T[] _items;
}