namespace FantaniaLib;

public class NewLevelEntityOperation : FrameBasedOperation
{
    public NewLevelEntityOperation(IWorkspace workspace, LevelEntity entity) : base(workspace.FrameCount)
    {
        // 由于Workspace在切换Level的时候会清除UndoStack，所以这里用LevelModule去操作应该不会有问题
        _entity = entity;
        _workspace = workspace;
    }

    public override void Redo()
    {
        _workspace.LevelModule.AddEntity(_entity);
    }

    public override void Undo()
    {
        _workspace.LevelModule.RemoveEntity(_entity);
    }

    LevelEntity _entity;
    IWorkspace _workspace;
}