using System.Collections;
using System.Collections.ObjectModel;
using System.Text;

namespace FantaniaLib;

public struct FantaniaArray<T> : IList<T>, IReadOnlyList<T>, ICustomSerializableField where T : notnull
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
        _items.CollectionChanged += OnItemsChanged;
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
        return _items.Remove(item);
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

    public void DeserializeFromString(string data, object instance)
    {
        int index = data.IndexOf('#');
        int count = int.Parse(data.Substring(0, index));
        int typeIndex = data.IndexOf('#', index + 1);
        FieldTypes fieldType = (FieldTypes)int.Parse(data.Substring(index + 1, typeIndex - index - 1));
        string valuesStr = data.Substring(typeIndex + 1);
        var rule = SerializationRule.Default;
        char separator = (char)30;
        string[] valueParts = valuesStr.Split(separator);
        _items.Clear();
        for (int i = 0; i < count; i++)
        {
            string itemData = valueParts[i];
            object item = rule.CastFrom(fieldType, itemData, this)!;
            _items.Add((T)item);
        }
    }

    public string SerializeToString(object instance)
    {
        var sb = new StringBuilder();
        sb.Append(Count.ToString());
        sb.Append('#');
        var type = typeof(T);
        FieldTypes fieldType = ConversionHelper.TypeToFieldType(type);
        sb.Append((int)fieldType);
        sb.Append('#');
        var rule = SerializationRule.Default;
        char separator = (char)30;
        for (int i = 0; i < _items.Count; i++)
        {
            string val = rule.CastTo(fieldType, _items[i]!, this);
            sb.Append(val);
            if (i < _items.Count - 1)
                sb.Append(separator);
        }
        return sb.ToString();
    }

    public static bool operator==(FantaniaArray<T> a, FantaniaArray<T> b)
    {
        if (a.Count != b.Count) return false;
        if (a.Notify != b.Notify) return false;
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
        return HashCode.Combine(Count, Notify);
    }

    void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Notify++;
    }

    ObservableCollection<T> _items = new ObservableCollection<T>();
    internal int Notify = 0;
}