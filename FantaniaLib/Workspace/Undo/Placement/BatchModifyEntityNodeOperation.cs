namespace FantaniaLib;

public class BatchModifyNodesOperation : FrameBasedOperation
{
    public BatchModifyNodesOperation(IWorkspace workspace, MultiNodesEntity entity, EntityNodesSnapshot snapshotBefore, EntityNodesSnapshot snapshotAfter)
        : base(workspace.FrameCount)
    {
        _workspace = workspace;
        _entity = entity;
        _snapshotBefore = snapshotBefore;
        _snapshotAfter = snapshotAfter;
    }

    public override void Undo()
    {
        _entity.SetNodes(_workspace, _snapshotBefore);
    }

    public override void Redo()
    {
        _entity.SetNodes(_workspace, _snapshotAfter);
    }

    IWorkspace _workspace;
    MultiNodesEntity _entity;
    EntityNodesSnapshot _snapshotBefore;
    EntityNodesSnapshot _snapshotAfter;
}