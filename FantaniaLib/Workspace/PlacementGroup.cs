
using System.Collections.ObjectModel;

namespace FantaniaLib;

public class PlacementGroup : IPlacement
{
    public string ClassName => null;
    public string Group => string.Empty;
    public string Name => _groupName;
    public string Tooltip => null;
    public IList<IPlacement> Children => _children;

    public PlacementGroup(string group)
    {
        _groupName = group;
    }

    string _groupName;
    ObservableCollection<IPlacement> _children = new ObservableCollection<IPlacement>();
}