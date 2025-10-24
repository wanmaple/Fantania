using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class DeleteLevelObjectOperation : FrameBasedOperation
{
    public DeleteLevelObjectOperation(LevelObject obj)
    {
        _lvObj = obj;
    }

    public override void Redo()
    {
        WorkspaceViewModel.Current.Workspace.UnwatchBVHItemChanged();
        _lvObj.IsSelected = false;
        WorkspaceViewModel.Current.Workspace.CurrentLevel.RemoveObject(_lvObj);
        WorkspaceViewModel.Current.Workspace.WatchBVHItemChanged();
        WorkspaceViewModel.Current.Workspace.CurrentLevel.MarkDirty();
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    public override void Undo()
    {
        WorkspaceViewModel.Current.Workspace.UnwatchBVHItemChanged();
        WorkspaceViewModel.Current.Workspace.CurrentLevel.AddObject(_lvObj);
        WorkspaceViewModel.Current.Workspace.WatchBVHItemChanged();
        WorkspaceViewModel.Current.Workspace.CurrentLevel.MarkDirty();
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    LevelObject _lvObj;
}