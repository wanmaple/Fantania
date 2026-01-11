namespace FantaniaLib;

public class NewDatabaseObjectOperation : FrameBasedOperation
{
    public NewDatabaseObjectOperation(IWorkspace workspace, DatabaseObject newObj) : base(workspace.FrameCount)
    {
        _workspace = workspace;
        _newObj = newObj;
    }

    public override void Redo()
    {
        _workspace.DatabaseModule.AddObject(_newObj);
    }

    public override void Undo()
    {
        _workspace.DatabaseModule.RemoveObject(_newObj);
    }

    IWorkspace _workspace;
    DatabaseObject _newObj;
}