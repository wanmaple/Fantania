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

    public IReadOnlyList<LocalRenderInfo> GetLocalNodeAt(UserPlacement placement, int index)
    {
        try
        {
            return GetOrCallMember("nodeAt", placement, index).ToObject<List<LocalRenderInfo>>();
        }
        catch
        {
            return Array.Empty<LocalRenderInfo>();
        }
    }

    ObservableCollection<IPlacement> _source = new ObservableCollection<IPlacement>();
    FilterableBindingSource<IPlacement> _filtered;
}