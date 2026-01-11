using System.Collections;
using System.Collections.Specialized;

namespace FantaniaLib;

public sealed class FilterableObservableCollection<T> : IList<T>, INotifyCollectionChanged
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IList<T> AllItems => _innerCollection;

    public FilterableObservableCollection(IList<T> innerCollection)
    {
        _innerCollection = innerCollection;
        _orders = new LinkedList<int>();
        Filter(null, null);
    }

    public int Count => _count;

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get
        {
            VerifyIndexRange(index);
            return _innerCollection[_orders.ElementAt(index)];
        }
        set
        {
            VerifyIndexRange(index);
            _innerCollection[_orders.ElementAt(index)] = value;
        }
    }

    private struct SortData
    {
        public int index;
        public string sortKey;
    }

    public void Filter(Predicate<T>? filterFunc, Func<T, string>? priorityFunc)
    {
        _count = 0;
        _orders.Clear();
        var priorities = new List<SortData>();
        int idx = 0;
        foreach (var item in _innerCollection)
        {
            if (filterFunc == null || filterFunc(item))
            {
                string sortKey = priorityFunc == null ? string.Empty : priorityFunc(item);
                priorities.Add(new SortData { index = idx, sortKey = sortKey });
            }
            ++idx;
        }
        priorities = priorities.OrderBy(item => item.sortKey).ToList();
        foreach (var data in priorities)
        {
            _orders.AddLast(data.index);
            ++_count;
        }
        _lastFilterFunc = filterFunc;
        _lastPriorityFunc = priorityFunc;

        RaiseCollectionChanged();
    }

    public void Add(T item)
    {
        _innerCollection.Add(item);
        Filter(_lastFilterFunc, _lastPriorityFunc);
    }

    public void Clear()
    {
        _count = 0;
        _orders.Clear();
        _innerCollection.Clear();

        RaiseCollectionChanged();
    }

    public bool Contains(T item)
    {
        return _innerCollection.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        for (int i = 0; i < _count; i++)
        {
            int idx = _orders.ElementAt(i);
            array[arrayIndex + i] = _innerCollection[idx];
        }
    }

    public bool Remove(T item)
    {
        bool removed = _innerCollection.Remove(item);
        if (removed)
        {
            Filter(_lastFilterFunc, _lastPriorityFunc);
        }
        return removed;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            int index = _orders.ElementAt(i);
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
        Filter(_lastFilterFunc, _lastPriorityFunc);
        RaiseCollectionChanged();
    }

    public void RemoveAt(int index)
    {
        VerifyIndexRange(index);
        LinkedListNode<int> curNode = _orders.First!;
        while (index > 0)
        {
            --index;
            curNode = curNode.Next!;
        }
        _innerCollection.RemoveAt(curNode.Value);
        Filter(_lastFilterFunc, _lastPriorityFunc);
    }

    private void RaiseCollectionChanged()
    {
        if (CollectionChanged != null)
        {
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private void VerifyIndexRange(Int32 index)
    {
        if (index < 0 || index >= _count)
        {
            throw new ArgumentOutOfRangeException("0 <= index < " + _count.ToString());
        }
    }

    private IList<T> _innerCollection;
    private LinkedList<int> _orders;
    private int _count;
    private Predicate<T>? _lastFilterFunc;
    private Func<T, string>? _lastPriorityFunc;
}