using System.Collections;
using System.Numerics;
using System.Text;

namespace FantaniaLib;

public class EntityNodeCollection : IReadOnlyList<LevelEntityNode>, IList<LevelEntityNode>, ICustomSerializableField
{
    public int Count => _nodes.Count;
    public bool IsReadOnly => false;

    LevelEntityNode IList<LevelEntityNode>.this[int index]
    {
        get => _nodes[index];
        set => _nodes[index] = value;
    }

    public LevelEntityNode this[int index] => _nodes[index];

    public EntityNodeCollection()
    {
    }

    public IEnumerator<LevelEntityNode> GetEnumerator()
    {
        return _nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(LevelEntityNode item)
    {
        return _nodes.IndexOf(item);
    }

    public void Insert(int index, LevelEntityNode item)
    {
        _nodes.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _nodes.RemoveAt(index);
    }

    public void Add(LevelEntityNode item)
    {
        _nodes.Add(item);
    }

    public void Clear()
    {
        _nodes.Clear();
    }

    public bool Contains(LevelEntityNode item)
    {
        return _nodes.Contains(item);
    }

    public void CopyTo(LevelEntityNode[] array, int arrayIndex)
    {
        _nodes.CopyTo(array, arrayIndex);
    }

    public bool Remove(LevelEntityNode item)
    {
        return _nodes.Remove(item);
    }

    public void AddRange(IEnumerable<LevelEntityNode> items)
    {
        _nodes.AddRange(items);
    }

    public string SerializeToString(object instance)
    {
        var sb = new StringBuilder();
        sb.Append(_nodes.Count);
        var rule = SerializationRule.Default;
        for (int i = 0; i < _nodes.Count; i++)
        {
            sb.Append(rule.CastTo(FieldTypes.Vector2Int, _nodes[i].Position, _nodes[i]));
            sb.Append(';');
            sb.Append(rule.CastTo(FieldTypes.Float, _nodes[i].Rotation, _nodes[i]));
            sb.Append(';');
            sb.Append(rule.CastTo(FieldTypes.Vector2, _nodes[i].Scale, _nodes[i]));
            sb.Append(';');
            sb.Append(rule.CastTo(FieldTypes.Integer, _nodes[i].NodeId, _nodes[i]));
            if (i < _nodes.Count - 1)
                sb.Append('|');
        }
        return sb.ToString();
    }

    public void DeserializeFromString(string data, object instance)
    {
        _nodes.Clear();
        var rule = SerializationRule.Default;
        string[] parts = data.Split('|');
        foreach (var part in parts)
        {
            string[] fields = part.Split(';');
            var node = new LevelEntityNode((IMultiNodeContainer)instance);
            node.Position = (Vector2Int)rule.CastFrom(FieldTypes.Vector2Int, fields[0], node)!;
            node.Rotation = (float)rule.CastFrom(FieldTypes.Float, fields[1], node)!;
            node.Scale = (Vector2)rule.CastFrom(FieldTypes.Vector2, fields[2], node)!;
            node.NodeId = (int)rule.CastFrom(FieldTypes.Integer, fields[3], node)!;
            _nodes.Add(node);
        }
    }

    List<LevelEntityNode> _nodes = new List<LevelEntityNode>(0);
}