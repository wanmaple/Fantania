using Fantania.ViewModels;

namespace Fantania;

public class DeleteDatabaseObjectOperation : FrameBasedOperation
{
    public DeleteDatabaseObjectOperation(DatabaseObject delObj)
    {
        _deleteObj = delObj;
    }

    public override void Redo()
    {
        WorkspaceViewModel.Current.Workspace.RemoveObject(_deleteObj, false);
    }

    public override void Undo()
    {
        WorkspaceViewModel.Current.Workspace.AddObject(_deleteObj, false);
    }

    DatabaseObject _deleteObj;
}