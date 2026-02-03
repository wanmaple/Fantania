
namespace FantaniaLib;

public class MultiNodesEntity : LevelEntity, IMultiNodeContainer
{
    public override int NodeCount => AllNodes.Count;
    public IReadOnlyList<LevelEntityNode> AllNodes => _nodes;

    public MultiNodesEntity(int nodeNum)
    {
        for (int i = 0; i < nodeNum; i++)
        {
            AppendNode();
        }
    }

    public void AppendNode()
    {
        _nodes.Add(new LevelEntityNode(this, _nodes.Count));
    }

    public bool CanRotate(IWorkspace workspace, int index)
    {
        return GetReferencedPlacement(workspace).Template.CanRotate(index);
    }

    public bool CanScale(IWorkspace workspace, int index)
    {
        return GetReferencedPlacement(workspace).Template.CanScale(index);
    }

    public bool CanTranslate(IWorkspace workspace, int index)
    {
        return GetReferencedPlacement(workspace).Template.CanTranslate(index);
    }

    public override void GetLocalNodeAt(IWorkspace workspace, int index, out IReadOnlyList<LocalRenderInfo> locals)
    {
        throw new NotImplementedException();
    }

    public override void OnAddSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index, Rectf bound)
    {
        throw new NotImplementedException();
    }

    public override void OnRemoveSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index)
    {
        throw new NotImplementedException();
    }

    public override void OnUpdateSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index)
    {
        throw new NotImplementedException();
    }

    public void RemoveNodeAt(int index)
    {
        throw new NotImplementedException();
    }

    public override void TryAppendNode()
    {
        AppendNode();
    }

    List<LevelEntityNode> _nodes = new List<LevelEntityNode>(8);
    bool _nodesDirty = true;
}