namespace FantaniaLib;

public interface IMultiNodeContainer
{
    IReadOnlyList<LevelEntityNode> AllNodes { get; }

    void AppendNode();
    void RemoveNodeAt(int index);
    bool CanTranslate(IWorkspace workspace, int index);
    bool CanRotate(IWorkspace workspace, int index);
    bool CanScale(IWorkspace workspace, int index);
}