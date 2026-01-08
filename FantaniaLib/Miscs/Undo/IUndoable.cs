namespace FantaniaLib;

public interface IUndoable
{
    public ulong Frame { get; }
    void Undo();
    void Redo();
    bool TryMerge(IUndoable other, out IUndoable merged);
}