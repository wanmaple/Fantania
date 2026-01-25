namespace FantaniaLib;

public class DelLevelEntityOperation : FrameBasedOperation
{
    public DelLevelEntityOperation(IWorkspace workspace, LevelEntity delEntity) : base(workspace.FrameCount)
    {
        _workspace = workspace;
        _delEntity = delEntity;
    }

    public override void Redo()
    {
        _workspace.LevelModule.RemoveEntity(_delEntity);
    }

    public override void Undo()
    {
        _workspace.LevelModule.AddEntity(_delEntity);
    }

    IWorkspace _workspace;
    LevelEntity _delEntity;
}