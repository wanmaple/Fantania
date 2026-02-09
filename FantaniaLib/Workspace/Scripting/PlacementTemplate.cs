using System.Collections.ObjectModel;
using MoonSharp.Interpreter;

namespace FantaniaLib;

public class PlacementTemplate : ScriptTemplate, IPlacement
{
    public IReadOnlyList<IPlacement> Children => _filtered;
    public IList<IPlacement> Source => _source;

    public PlacementTypes PlacementType => GetOrCallMember("placementType").GetEnumOrDefault(PlacementTypes.Single);
    public int DefaultLayer => GetOrCallMember("defaultLayer").GetIntegerOrDefault(0);
    public NodeOptions NodeOptions => GetOrCallMember("nodeOptions").GetObjectOrDefault(NodeOptions.Default);
    public Vector2Int TileSize => GetOrCallMember("tileSize").GetObjectOrDefault(Vector2Int.Zero);

    public PlacementTemplate(ScriptEngine engine, DynValue obj) : base(engine, obj)
    {
        _filtered = new FilterableBindingSource<IPlacement>(_source);
    }

    public bool CanTranslate(int index)
    {
        return GetOrCallMember("canTranslate", index).GetBooleanOrDefault(true);
    }

    public bool CanRotate(int index)
    {
        return GetOrCallMember("canRotate", index).GetBooleanOrDefault(true);
    }

    public bool CanScale(int index)
    {
        return GetOrCallMember("canScale", index).GetBooleanOrDefault(true);
    }

    public IReadOnlyList<LocalRenderInfo> GetLocalNodeAt(UserPlacement placement, int index, int nodeCnt)
    {
        try
        {
            return GetOrCallMember("nodeAt", placement, index, nodeCnt).GetObjectOrDefault(new List<LocalRenderInfo>());
        }
        catch
        {
            return Array.Empty<LocalRenderInfo>();
        }
    }

    public IReadOnlyList<LocalRenderInfo> GetBackgroundNodes(UserPlacement placement, IReadOnlyList<LevelEntityNode> nodes)
    {
        try
        {
            return GetOrCallMember("backgroundNodes", placement, nodes.ToList()).GetObjectOrDefault(new List<LocalRenderInfo>());
        }
        catch
        {
            return Array.Empty<LocalRenderInfo>();
        }
    }

    public IReadOnlyList<LocalRenderInfo> GetForegroundNodes(UserPlacement placement, IReadOnlyList<LevelEntityNode> nodes)
    {
        try
        {
            return GetOrCallMember("foregroundNodes", placement, nodes.ToList()).GetObjectOrDefault(new List<LocalRenderInfo>());
        }
        catch
        {
            return Array.Empty<LocalRenderInfo>();
        }
    }

    public TileInfo GetTileInfo(UserPlacement placement, Vector2Int size, int x, int y)
    {
        try
        {
            return GetOrCallMember("tileAt", placement, size, x, y).GetObjectOrDefault(TileInfo.Default);
        }
        catch
        {
            return TileInfo.Default;
        }
    }

    ObservableCollection<IPlacement> _source = new ObservableCollection<IPlacement>();
    FilterableBindingSource<IPlacement> _filtered;
}