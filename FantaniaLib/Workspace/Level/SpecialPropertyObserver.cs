using System.ComponentModel;

namespace FantaniaLib;

public class SpecialPropertyObserver
{
    public bool HasChange => _changedEntities.Count > 0;

    public void Observe(MultiNodesEntity entity)
    {
        var rule = SerializationRule.Default;
        _snapshots.Add(entity, rule.CastTo(FieldTypes.Custom, entity.MutableNodes, entity));
        entity.NodeAdded += OnEntityNodeAdded;
        entity.NodeRemoved += OnEntityNodeRemoved;
        foreach (var node in entity.AllNodes)
        {
            node.PropertyChanged += OnNodePropertyChanged;
        }
    }

    public void Reset()
    {
        foreach (var entity in _snapshots.Keys)
        {
            entity.NodeAdded -= OnEntityNodeAdded;
            entity.NodeRemoved -= OnEntityNodeRemoved;
            foreach (var node in entity.AllNodes)
            {
                node.PropertyChanged -= OnNodePropertyChanged;
            }
        }
        _snapshots.Clear();
        _changedEntities.Clear();
    }

    void OnEntityNodeAdded(LevelEntityNode node)
    {
        MultiNodesEntity entity = (MultiNodesEntity)node.Owner;
        var snapshotBefore = _snapshots[entity];
        var snapshotAfter = SerializationRule.Default.CastTo(FieldTypes.Custom, entity.MutableNodes, entity);
        if (!_changedEntities.Contains(entity))
        {
            if (snapshotBefore != snapshotAfter)
            {
                _changedEntities.Add(entity);
            }
        }
        else
        {
            if (snapshotBefore == snapshotAfter)
            {
                _changedEntities.Remove(entity);
            }
        }
    }

    void OnEntityNodeRemoved(LevelEntityNode node)
    {
        MultiNodesEntity entity = (MultiNodesEntity)node.Owner;
        var snapshotBefore = _snapshots[entity];
        // 因为是延迟删除，所以实际上MutableNodes里还没有被删除，需要克隆一份来计算snapshotAfter
        var clone = entity.MutableNodes.Clone();
        clone.Remove(node);
        var snapshotAfter = SerializationRule.Default.CastTo(FieldTypes.Custom, clone, entity);
        if (!_changedEntities.Contains(entity))
        {
            if (snapshotBefore != snapshotAfter)
            {
                _changedEntities.Add(entity);
            }
        }
        else
        {
            if (snapshotBefore == snapshotAfter)
            {
                _changedEntities.Remove(entity);
            }
        }
    }

    void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LevelEntityNode.Position) || e.PropertyName == nameof(LevelEntityNode.Rotation) || e.PropertyName == nameof(LevelEntityNode.Scale))
        {
            LevelEntityNode node = (LevelEntityNode)sender!;
            MultiNodesEntity entity = (MultiNodesEntity)node.Owner;
            OnEntityNodeAdded(node);
        }
    }

    Dictionary<MultiNodesEntity, string> _snapshots = new Dictionary<MultiNodesEntity, string>();
    HashSet<MultiNodesEntity> _changedEntities = new HashSet<MultiNodesEntity>();
}