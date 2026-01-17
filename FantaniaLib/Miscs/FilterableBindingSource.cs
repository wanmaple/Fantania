using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FantaniaLib;

public sealed class FilterableBindingSource<T> : ObservableCollection<T>, IDisposable
{
    struct SortData
    {
        public int index;
        public string sortKey;
    }

    public FilterableBindingSource(IList<T> source)
    {
        _source = source;
        if (_source is INotifyCollectionChanged incc)
            incc.CollectionChanged += OnSourceCollectionChanged;
        Filter(null, null);
    }

    ~FilterableBindingSource()
    {
        Dispose();
    }

    public void Filter(Predicate<T>? filterFunc, Func<T, string>? sortFunc)
    {
        Clear();
        var priorities = new List<SortData>();
        int idx = 0;
        foreach (var item in _source)
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
            Add(_source[data.index]);
        }
        _lastFilterFunc = filterFunc;
        _lastSortFunc = sortFunc;
    }

    public void Dispose()
    {
        if (_source is INotifyCollectionChanged incc)
            incc.CollectionChanged -= OnSourceCollectionChanged;
    }

    void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Filter(_lastFilterFunc, _lastSortFunc);
    }

    IList<T> _source;
    Predicate<T>? _lastFilterFunc;
    Func<T, string>? _lastSortFunc;
}