namespace FantaniaLib;

public class UndoableGroup : FrameBasedOperation
{
    public UndoableGroup(ulong frame) : base(frame)
    {
        _ops = new List<IUndoable>(16);
    }

    public void MergeOperation(IUndoable op)
    {
        _ops.Add(op);
    }

    public override void Undo()
    {
        int num = _ops.Count;
        for (int i = 0; i < num; i++)
        {
            _ops[i].Undo();
        }
        if (_ops.Count != num)
        {
            throw new InvalidOperationException("Some Operation modify the undo.");
        }
    }

    public override void Redo()
    {
        foreach (IUndoable op in _ops)
            op.Redo();
    }

    public override bool TryMerge(IUndoable other, out IUndoable? merged)
    {
        if (Frame == other.Frame)
        {
            _ops.Add(other);
            merged = this;
            return true;
        }
        for (int i = 0; i < _ops.Count; i++)
        {
            IUndoable op = _ops[i];
            if (op.TryMerge(other, out merged))
            {
                _ops[i] = merged!;
                merged = this;
                return true;
            }
        }
        merged = null;
        return false;
    }

    IList<IUndoable> _ops;
}