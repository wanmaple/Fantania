using System.Collections.ObjectModel;
using System.Numerics;
using MoonSharp.Interpreter;

namespace FantaniaLib;

public class PlacementTemplate : ScriptTemplate, IPlacement
{
    public IReadOnlyList<IPlacement> Children => _filtered;
    public IList<IPlacement> Source => _source;

    public Vector2 DefaultAnchor => GetOrCallMember("defaultAnchor").GetObjectOrDefault(new Vector2(0.5f, 1.0f));
    public int DefaultLayer => GetOrCallMember("defaultLayer").GetIntegerOrDefault(0);
    public NodeOptions NodeOptions => GetOrCallMember("nodeOptions").GetObjectOrDefault(new NodeOptions
    {
        Minimum = 0,
        Maximum = 0,
        DefaultOffset = new Vector2Int(32, 0),
    });

    public PlacementTemplate(ScriptEngine engine, DynValue obj) : base(engine, obj)
    {
        _filtered = new FilterableBindingSource<IPlacement>(_source);
    }

    public IReadOnlyList<ScriptRenderInfo> GetRenderables(LevelEntity entity)
    {
        return GetOrCallMember("renderables", entity).ToObject<List<ScriptRenderInfo>>();
    }

    ObservableCollection<IPlacement> _source = new ObservableCollection<IPlacement>();
    FilterableBindingSource<IPlacement> _filtered;
}