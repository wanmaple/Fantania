namespace FantaniaLib;

public class ModifyLevelEntityOperation : FrameBasedOperation
{
    public ModifyLevelEntityOperation(IWorkspace workspace, LevelEntity entity, PropertyChangeInfo change) : base(workspace.FrameCount)
    {
        _workspace = workspace;
        _modifyEntity = entity;
        _change = change;
    }

    public override void Redo()
    {
        FieldInfo field = _modifyEntity.SerializableFields.First(f => f.FieldName == _change.PropertyName);
        object? newVal = _workspace.DatabaseModule.SerializationRule.CastFrom(field.FieldType, _change.NewValue, _modifyEntity);
        _workspace.LevelModule.UnwatchPropertyChange(_modifyEntity);
        _modifyEntity.SetFieldValue(_change.PropertyName, newVal);
        _workspace.LevelModule.WatchPropertyChange(_modifyEntity);
    }

    public override void Undo()
    {
        FieldInfo field = _modifyEntity.SerializableFields.First(f => f.FieldName == _change.PropertyName);
        object? oldVal = _workspace.DatabaseModule.SerializationRule.CastFrom(field.FieldType, _change.OldValue, _modifyEntity);
        _workspace.LevelModule.UnwatchPropertyChange(_modifyEntity);
        _modifyEntity.SetFieldValue(_change.PropertyName, oldVal);
        _workspace.LevelModule.WatchPropertyChange(_modifyEntity);
    }

    public override bool TryMerge(IUndoable other, out IUndoable? merged)
    {
        if (other is ModifyLevelEntityOperation modifying && _modifyEntity == modifying._modifyEntity && _change.PropertyName == modifying._change.PropertyName)
        {
            _change.NewValue = modifying._change.NewValue;
            merged = this;
            return true;
        }
        return base.TryMerge(other, out merged);
    }

    IWorkspace _workspace;
    LevelEntity _modifyEntity;
    PropertyChangeInfo _change;
}