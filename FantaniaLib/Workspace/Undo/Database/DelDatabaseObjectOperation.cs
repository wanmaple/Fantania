namespace FantaniaLib;

public class DelDatabaseObjectOperation : FrameBasedOperation
{
    public DelDatabaseObjectOperation(IWorkspace workspace, DatabaseObject delObj) : base(workspace.FrameCount)
    {
        _workspace = workspace;
        _delObj = delObj;
    }

    public override void Redo()
    {
        _workspace.DatabaseModule.RemoveObject(_delObj);
    }

    public override void Undo()
    {
        _workspace.DatabaseModule.AddObject(_delObj);
    }

    IWorkspace _workspace;
    DatabaseObject _delObj;
}