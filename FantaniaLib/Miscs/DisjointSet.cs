namespace FantaniaLib;

public sealed class DisjointSet<T> where T : notnull
{
    public DisjointSet(IReadOnlyList<T> source)
    {
        _indexCache = new Dictionary<T, int>(source.Count);
        _source = source;
        Initialize(_source);
        for (int i = 0; i < source.Count; i++)
        {
            _parent[i] = i;
            _indexCache.Add(_source[i], i);
        }
    }

    void Initialize(IReadOnlyList<T> source)
    {
        int size = source.Count;
        _parent = new int[size];
        _rank = new int[size];
    }

    public void Reset(IReadOnlyList<T> newSource)
    {
        if (newSource.Count > _source.Count)
        {
            Initialize(newSource);
        }
        _source = newSource;
        _indexCache.Clear();
        for (int i = 0; i < newSource.Count; i++)
        {
            _parent[i] = i;
            _indexCache.Add(_source[i], i);
        }
    }

    public T Find(T item)
    {
        if (!_indexCache.TryGetValue(item, out int index))
            throw new ArgumentException("Item not found in disjoint set.");
        int rootIndex = Find(index);
        return _source[rootIndex];
    }

    public void Union(T a, T b)
    {
        if (!_indexCache.TryGetValue(a, out int indexA) || !_indexCache.TryGetValue(b, out int indexB))
            throw new ArgumentException("Item not found in disjoint set.");
        Union(indexA, indexB);
    }

    int Find(int x)
    {
        if (_parent[x] != x)
            _parent[x] = Find(_parent[x]);
        return _parent[x];
    }

    void Union(int a, int b)
    {
        int rootA = Find(a);
        int rootB = Find(b);
        if (rootA == rootB) return;
        if (_rank[rootA] < _rank[rootB])
        {
            _parent[rootA] = rootB;
        }
        else if (_rank[rootA] > _rank[rootB])
        {
            _parent[rootB] = rootA;
        }
        else
        {
            _parent[rootB] = rootA;
            _rank[rootA]++;
        }
    }

    IReadOnlyList<T> _source;
    int[] _parent = Array.Empty<int>();
    int[] _rank = Array.Empty<int>();
    readonly Dictionary<T, int> _indexCache;
}