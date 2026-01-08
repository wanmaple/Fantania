namespace FantaniaLib;

public abstract class FrameBasedOperation : IUndoable
{
    public ulong Frame { get; private set; }

    protected FrameBasedOperation(ulong frame)
    {
        Frame = frame;
    }

    public abstract void Undo();
    public abstract void Redo();

    public virtual bool TryMerge(IUndoable other, out IUndoable merged)
    {
        if (Frame == other.Frame)
        {
            var group = new UndoableGroup(Frame);
            group.MergeOperation(this);
            group.MergeOperation(other);
            merged = group;
            return true;
        }
        merged = null;
        return false;
    }
}