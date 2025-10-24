using Fantania.ViewModels;

namespace Fantania;

public class NewDatabaseObjectOperation : FrameBasedOperation
{    public NewDatabaseObjectOperation(DatabaseObject newObj)
    {
        _newObject = newObj;
    }

    public override void Redo()
    {
        WorkspaceViewModel.Current.Workspace.AddObject(_newObject, false);
    }

    public override void Undo()
    {
        WorkspaceViewModel.Current.Workspace.RemoveObject(_newObject, false);
    }

    DatabaseObject _newObject;
}