using System.Numerics;

namespace FantaniaLib;

public interface IMultiNodeContainer
{
    event Action<LevelEntityNode>? NodeAdded;
    event Action<LevelEntityNode>? NodeRemoved;

    IReadOnlyList<LevelEntityNode> AllNodes { get; }
    int Depth { get; }
    int Order { get; }
    Matrix3x3 SelfTransform { get; }

    void AppendNode(IWorkspace workspace, Vector2? worldPos);
    bool RemoveNodes(IWorkspace workspace, EntityNodesSnapshot snapshot);
    bool CanTranslate(IWorkspace workspace, int index);
    bool CanRotate(IWorkspace workspace, int index);
    bool CanScale(IWorkspace workspace, int index);
    Matrix3x3 TransformAt(int index);
    void MarkTransformDirty();
}