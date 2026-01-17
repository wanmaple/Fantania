
using System.Collections.ObjectModel;

namespace FantaniaLib;

public class PlacementGroup : IPlacement
{
    public string ClassName => string.Empty;
    public string Group => string.Empty;
    public string Name => _groupName;
    public string Tooltip => string.Empty;
    public IReadOnlyList<IPlacement> Children => _source;
    public IList<IPlacement> Source => _source;

    public PlacementGroup(string group)
    {
        _groupName = group;
    }

    string _groupName;
    ObservableCollection<IPlacement> _source = new ObservableCollection<IPlacement>();
}