using System.Collections.Generic;
using FantaniaLib;

namespace Fantania;

public struct SelectionGroups
{
    public List<MultiNodesEntity> FullySelectedEntities;
    public Dictionary<IMultiNodeContainer, List<LevelEntityNode>> PartiallySelectedNodes;
    public List<ISelectableItem> OtherSelectables;
}

public static class SelectionHelper
{
    public static SelectionGroups GroupSelections(IList<ISelectableItem> selections)
    {
        var result = new SelectionGroups
        {
            FullySelectedEntities = new List<MultiNodesEntity>(),
            PartiallySelectedNodes = new Dictionary<IMultiNodeContainer, List<LevelEntityNode>>(),
            OtherSelectables = new List<ISelectableItem>()
        };
        var nodesByOwner = new Dictionary<IMultiNodeContainer, List<LevelEntityNode>>();
        foreach (var sel in selections)
        {
            if (sel is LevelEntityNode node)
            {
                if (!nodesByOwner.TryGetValue(node.Owner, out var nodeList))
                {
                    nodeList = new List<LevelEntityNode>();
                    nodesByOwner.Add(node.Owner, nodeList);
                }
                nodeList.Add(node);
            }
            else
            {
                result.OtherSelectables.Add(sel);
            }
        }
        foreach (var (owner, nodes) in nodesByOwner)
        {
            if (owner is MultiNodesEntity entity && nodes.Count == entity.AllNodes.Count)
            {
                // 所有子节点都被选中
                result.FullySelectedEntities.Add(entity);
            }
            else
            {
                // 部分子节点被选中
                result.PartiallySelectedNodes.Add(owner, nodes);
            }
        }
        return result;
    }
}
