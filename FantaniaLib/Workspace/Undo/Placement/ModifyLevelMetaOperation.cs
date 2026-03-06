namespace FantaniaLib;

public class ModifyLevelMetaOperation : FrameBasedOperation
{
    public ModifyLevelMetaOperation(IWorkspace workspace, LevelMetadata meta, PropertyChangeInfo change) : base(workspace.FrameCount)
    {
        _workspace = workspace;
        _meta = meta;
        _change = change;
    }

    public override void Redo()
    {
        FieldInfo field = _meta.SerializableFields.First(f => f.FieldName == _change.PropertyName);
        object? newVal = _workspace.DatabaseModule.SerializationRule.CastFrom(field.FieldType, _change.NewValue, _meta);
        _workspace.LevelModule.UnwatchMetaPropertyChange(_meta);
        _meta.SetFieldValue(_change.PropertyName, newVal);
        _workspace.LevelModule.WatchMetaPropertyChange(_meta);
    }

    public override void Undo()
    {
        FieldInfo field = _meta.SerializableFields.First(f => f.FieldName == _change.PropertyName);
        object? oldVal = _workspace.DatabaseModule.SerializationRule.CastFrom(field.FieldType, _change.OldValue, _meta);
        _workspace.LevelModule.UnwatchMetaPropertyChange(_meta);
        _meta.SetFieldValue(_change.PropertyName, oldVal);
        _workspace.LevelModule.WatchMetaPropertyChange(_meta);
    }

    IWorkspace _workspace;
    LevelMetadata _meta;
    PropertyChangeInfo _change;
}