namespace FantaniaLib;

public class EntityNodesSnapshot
{
    public IReadOnlyList<LevelEntityNode> Nodes { get; set; }

    public EntityNodesSnapshot(IReadOnlyList<LevelEntityNode> nodes)
    {
        Nodes = nodes.ToList();
    }
}