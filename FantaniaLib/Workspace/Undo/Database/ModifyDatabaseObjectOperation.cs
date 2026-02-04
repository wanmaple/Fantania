namespace FantaniaLib;

public class ModifyDatabaseObjectOperation : FrameBasedOperation
{
    public ModifyDatabaseObjectOperation(IWorkspace workspace, DatabaseObject obj, PropertyChangeInfo change) : base(workspace.FrameCount)
    {
        _workspace = workspace;
        _modifyObj = obj;
        _change = change;
    }

    public override void Redo()
    {
        FieldInfo field = _modifyObj.SerializableFields.First(f => f.FieldName == _change.PropertyName);
        object? newVal = _workspace.DatabaseModule.SerializationRule.CastFrom(field.FieldType, _change.NewValue, _modifyObj);
        _workspace.DatabaseModule.UnwatchPropertyChange(_modifyObj);
        _modifyObj.SetFieldValue(_change.PropertyName, newVal);
        _workspace.DatabaseModule.WatchPropertyChange(_modifyObj);
    }

    public override void Undo()
    {
        FieldInfo field = _modifyObj.SerializableFields.First(f => f.FieldName == _change.PropertyName);
        object? oldVal = _workspace.DatabaseModule.SerializationRule.CastFrom(field.FieldType, _change.OldValue, _modifyObj);
        _workspace.DatabaseModule.UnwatchPropertyChange(_modifyObj);
        _modifyObj.SetFieldValue(_change.PropertyName, oldVal);
        _workspace.DatabaseModule.WatchPropertyChange(_modifyObj);
    }

    public override bool TryMerge(IUndoable other, out IUndoable? merged)
    {
        if (other is ModifyDatabaseObjectOperation modifying && _modifyObj == modifying._modifyObj && _change.PropertyName == modifying._change.PropertyName)
        {
            _change.NewValue = modifying._change.NewValue;
            merged = this;
            return true;
        }
        return base.TryMerge(other, out merged);
    }

    IWorkspace _workspace;
    DatabaseObject _modifyObj;
    PropertyChangeInfo _change;
}