namespace FantaniaLib;

public class UserPlacement : ScriptDatabaseObject, IPlacement
{
    public string ClassName => string.Empty;
    public string Group => string.Empty;

    public IReadOnlyList<IPlacement> Children => Array.Empty<IPlacement>();
    public IList<IPlacement> Source => Array.Empty<IPlacement>();

    public override string TypeName => TemplateAs<PlacementTemplate>().ClassName.MakeFirstCharacterUpper();
    public override string GroupName => string.Empty;

    public UserPlacement(PlacementTemplate template, int id) : base(template, id)
    {
    }

    public override string GetDisplayName(IWorkspace workspace)
    {
        return workspace.LocalizeString(_script.Name);
    }

    public IReadOnlyList<LocalRenderInfo> GetLocalNodeAt(int index, int nodeCnt)
    {
        return TemplateAs<PlacementTemplate>().GetLocalNodeAt(this, index, nodeCnt);
    }

    public IReadOnlyList<LocalRenderInfo> GetBackgroundNodes(IReadOnlyList<LevelEntityNode> nodes)
    {
        return TemplateAs<PlacementTemplate>().GetBackgroundNodes(this, nodes);
    }

    public IReadOnlyList<LocalRenderInfo> GetForegroundNodes(IReadOnlyList<LevelEntityNode> nodes)
    {
        return TemplateAs<PlacementTemplate>().GetForegroundNodes(this, nodes);
    }
}