using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class DeleteWorldObjectOperation : FrameBasedOperation
{
    public DeleteWorldObjectOperation(WorldObject obj)
    {
        _worldObj = obj;
    }

    public override void Redo()
    {
        WorkspaceViewModel.Current.Workspace.UnwatchBVHItemChanged();
        _worldObj.IsSelected = false;
        WorkspaceViewModel.Current.Workspace.CurrentWorld.RemoveObject(_worldObj);
        WorkspaceViewModel.Current.Workspace.WatchBVHItemChanged();
        WorkspaceViewModel.Current.Workspace.CurrentWorld.MarkDirty();
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    public override void Undo()
    {
        WorkspaceViewModel.Current.Workspace.UnwatchBVHItemChanged();
        WorkspaceViewModel.Current.Workspace.CurrentWorld.AddObject(_worldObj);
        WorkspaceViewModel.Current.Workspace.WatchBVHItemChanged();
        WorkspaceViewModel.Current.Workspace.CurrentWorld.MarkDirty();
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    WorldObject _worldObj;
}