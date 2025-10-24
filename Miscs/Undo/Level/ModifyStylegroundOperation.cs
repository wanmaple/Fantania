using System.Reflection;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class ModifyStylegroundOperation : FrameBasedOperation
{
    public ModifyStylegroundOperation(Styleground obj, PropertyChangeInfo change)
    {
        _modifyObject = obj;
        _change = change;
    }

    public override void Redo()
    {
        var propInfo = _modifyObject.GetType().GetProperty(_change.PropertyName, BindingFlags.Instance | BindingFlags.Public);
        var obj = SerializationHelper.TakeSerializableProperty(propInfo, _change.NewValue, WorkspaceViewModel.Current.Workspace);
        WorkspaceViewModel.Current.Workspace.UnwatchStylegroundPropertyChange(_modifyObject);
        propInfo.SetValue(_modifyObject, obj);
        WorkspaceViewModel.Current.Workspace.WatchStylegroundPropertyChange(_modifyObject);
        WorkspaceViewModel.Current.Workspace.CurrentStylegrounds.IsModified = true;
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    public override void Undo()
    {
        var propInfo = _modifyObject.GetType().GetProperty(_change.PropertyName, BindingFlags.Instance | BindingFlags.Public);
        var obj = SerializationHelper.TakeSerializableProperty(propInfo, _change.OldValue, WorkspaceViewModel.Current.Workspace);
        WorkspaceViewModel.Current.Workspace.UnwatchStylegroundPropertyChange(_modifyObject);
        propInfo.SetValue(_modifyObject, obj);
        WorkspaceViewModel.Current.Workspace.WatchStylegroundPropertyChange(_modifyObject);
        WorkspaceViewModel.Current.Workspace.CurrentStylegrounds.IsModified = true;
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    public override bool TryMerge(IUndoable other, out IUndoable merged)
    {
        if (other is ModifyStylegroundOperation modifying && _modifyObject == modifying._modifyObject && _change.PropertyName == modifying._change.PropertyName)
        {
            _change.NewValue = modifying._change.NewValue;
            merged = this;
            return true;
        }
        return base.TryMerge(other, out merged);
    }

    Styleground _modifyObject;
    PropertyChangeInfo _change;
}