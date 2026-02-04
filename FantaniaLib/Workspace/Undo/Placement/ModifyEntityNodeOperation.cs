using System.Numerics;

namespace FantaniaLib;

public struct EntityNodeSnapshot
{
    public Vector2Int Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Scale { get; set; }
}

public class ModifyEntityNodeOperation : FrameBasedOperation
{
    public ModifyEntityNodeOperation(IWorkspace workspace, LevelEntityNode node, EntityNodeSnapshot snapshotBefore, EntityNodeSnapshot snapshotAfter)
        : base(workspace.FrameCount)
    {
        _node = node;
        _snapshotBefore = snapshotBefore;
        _snapshotAfter = snapshotAfter;
    }

    public override void Undo()
    {
        _node.ApplySnapshot(_snapshotBefore);
    }

    public override void Redo()
    {
        _node.ApplySnapshot(_snapshotAfter);
    }

    public override bool TryMerge(IUndoable other, out IUndoable? merged)
    {
        if (other is ModifyEntityNodeOperation modifying && _node == modifying._node)
        {
            _snapshotAfter = modifying._snapshotAfter;
            merged = this;
            return true;
        }
        return base.TryMerge(other, out merged);
    }

    LevelEntityNode _node;
    EntityNodeSnapshot _snapshotBefore;
    EntityNodeSnapshot _snapshotAfter;
}