using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FantaniaLib;

public sealed class FilterableObservableCollection<T> : IList<T>, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    struct SortData
    {
        public int index;
        public string sortKey;
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Count => _count;
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get
        {
            VerifyIndexRange(index);
            return _innerCollection[_orders[index]];
        }
        set
        {
            VerifyIndexRange(index);
            _innerCollection[_orders[index]] = value;
            Filter(_lastFilterFunc, _lastSortFunc);
        }
    }

    public FilterableObservableCollection(IList<T> innerCollection)
    {
        _innerCollection = innerCollection;
        _orders = new List<int>();
        Filter(null, null);
    }

    public void Filter(Predicate<T>? filterFunc, Func<T, string>? sortFunc)
    {
        _count = 0;
        _orders.Clear();
        var priorities = new List<SortData>();
        int idx = 0;
        foreach (var item in _innerCollection)
        {
            if (filterFunc == null || filterFunc(item))
            {
                string sortKey = sortFunc == null ? string.Empty : sortFunc(item);
                priorities.Add(new SortData { index = idx, sortKey = sortKey });
            }
            ++idx;
        }
        priorities.Sort((lhs, rhs) => lhs.sortKey.CompareTo(rhs.sortKey));
        foreach (var data in priorities)
        {
            _orders.Add(data.index);
            ++_count;
        }
        _lastFilterFunc = filterFunc;
        _lastSortFunc = sortFunc;

        OnCollectionChanged();
    }

    public void Add(T item)
    {
        _innerCollection.Add(item);
        Filter(_lastFilterFunc, _lastSortFunc);
    }

    public void Clear()
    {
        _count = 0;
        _orders.Clear();
        _innerCollection.Clear();
        OnCollectionChanged();
    }

    public bool Contains(T item)
    {
        return _innerCollection.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        for (int i = 0; i < _count; i++)
        {
            int idx = _orders[i];
            array[arrayIndex + i] = _innerCollection[idx];
        }
    }

    public bool Remove(T item)
    {
        bool removed = _innerCollection.Remove(item);
        if (removed)
        {
            Filter(_lastFilterFunc, _lastSortFunc);
        }
        return removed;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            int index = _orders[i];
            yield return _innerCollection[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return _innerCollection.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        VerifyIndexRange(index);
        _innerCollection.Insert(index, item);
        Filter(_lastFilterFunc, _lastSortFunc);
    }

    public void RemoveAt(int index)
    {
        VerifyIndexRange(index);
        _innerCollection.RemoveAt(_orders[index]);
        Filter(_lastFilterFunc, _lastSortFunc);
    }

    void OnCollectionChanged()
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    void VerifyIndexRange(int index)
    {
        if (index < 0 || index >= _count)
        {
            throw new ArgumentOutOfRangeException("0 <= index < " + _count.ToString());
        }
    }

    IList<T> _innerCollection;
    List<int> _orders;
    int _count;
    Predicate<T>? _lastFilterFunc;
    Func<T, string>? _lastSortFunc;
}