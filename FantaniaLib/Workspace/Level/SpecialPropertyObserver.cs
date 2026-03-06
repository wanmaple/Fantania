using System.ComponentModel;

namespace FantaniaLib;

public class SpecialPropertyObserver
{
    public bool HasChange => _changedEntities.Count > 0 || _metaChangeInfos.Count > 0;

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

    public void Observe(LevelMetadata meta)
    {
        _lvMeta = meta;
        _lvMeta.PropertyChanging += OnMetaPropertyChanging;
        _lvMeta.PropertyChanged += OnMetaPropertyChanged;
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
        if (_lvMeta != null)
        {
            _lvMeta.PropertyChanging -= OnMetaPropertyChanging;
            _lvMeta.PropertyChanged -= OnMetaPropertyChanged;
            _lvMeta = null;
        }
        _metaChangeInfos.Clear();
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

    void OnMetaPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        var fields = _lvMeta!.SerializableFields;
        var field = fields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (field != null)
        {
            LevelMetadata meta = (LevelMetadata)sender!;
            if (!_metaChangeInfos.TryGetValue(field.FieldName, out var changeInfo))
            {
                changeInfo = new PropertyChangeInfo
                {
                    OldValue = SerializationRule.Default.CastTo(field.FieldType, _lvMeta.GetFieldValue(field.FieldName), _lvMeta),
                };
                _metaChangeInfos.Add(field.FieldName, changeInfo);
            }
        }
    }

    void OnMetaPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var fields = _lvMeta!.SerializableFields;
        var field = fields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (field != null)
        {
            var changeInfo = _metaChangeInfos[field.FieldName];
            string newValue = SerializationRule.Default.CastTo(field.FieldType, _lvMeta.GetFieldValue(field.FieldName), _lvMeta);
            if (newValue == changeInfo.OldValue)
                _metaChangeInfos.Remove(field.FieldName);
            else
                changeInfo.NewValue = SerializationRule.Default.CastTo(field.FieldType, _lvMeta.GetFieldValue(field.FieldName), _lvMeta);
        }
    }

    Dictionary<MultiNodesEntity, string> _snapshots = new Dictionary<MultiNodesEntity, string>();
    HashSet<MultiNodesEntity> _changedEntities = new HashSet<MultiNodesEntity>();
    LevelMetadata? _lvMeta = null;
    Dictionary<string, PropertyChangeInfo> _metaChangeInfos = new Dictionary<string, PropertyChangeInfo>();
}