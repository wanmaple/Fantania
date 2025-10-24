using System;
using Fantania.ViewModels;

namespace Fantania;

public class ModifyDatabaseObjectOperation : FrameBasedOperation
{
    public ModifyDatabaseObjectOperation(DatabaseObject obj, PropertyChangeInfo change)
    {
        _modifyObject = obj;
        _change = change;
    }

    public override void Redo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        var dbSync = workspace.DatabaseSync;
        Type dbPropType = dbSync.GetDatabasePropertyType(_modifyObject.GetType(), _change.PropertyName);
        object newValue = DatabaseObjectSync.CommandText2DatabaseValue(dbPropType, _change.NewValue.ToString(), true);
        var propInfo = _modifyObject.GetType().GetProperty(_change.PropertyName);
        workspace.UnwatchDatabaseObjectPropertyChange(_modifyObject);
        propInfo.SetValue(_modifyObject, newValue);
        workspace.WatchDatabaseObjectPropertyChange(_modifyObject);
    }

    public override void Undo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        var dbSync = workspace.DatabaseSync;
        Type dbPropType = dbSync.GetDatabasePropertyType(_modifyObject.GetType(), _change.PropertyName);
        object oldValue = DatabaseObjectSync.CommandText2DatabaseValue(dbPropType, _change.OldValue.ToString(), true);
        var propInfo = _modifyObject.GetType().GetProperty(_change.PropertyName);
        workspace.UnwatchDatabaseObjectPropertyChange(_modifyObject);
        propInfo.SetValue(_modifyObject, oldValue);
        workspace.WatchDatabaseObjectPropertyChange(_modifyObject);
    }

    public override bool TryMerge(IUndoable other, out IUndoable merged)
    {
        if (other is ModifyDatabaseObjectOperation modifying && _modifyObject == modifying._modifyObject && _change.PropertyName == modifying._change.PropertyName)
        {
            _change.NewValue = modifying._change.NewValue;
            merged = this;
            return true;
        }
        return base.TryMerge(other, out merged);
    }

    DatabaseObject _modifyObject;
    PropertyChangeInfo _change;
}