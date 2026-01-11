
using System.Collections.ObjectModel;

namespace FantaniaLib;

public class PlacementGroup : IPlacement
{
    public string ClassName => string.Empty;
    public string Group => string.Empty;
    public string Name => _groupName;
    public string Tooltip => string.Empty;
    public IList<IPlacement> Children => _children;

    public PlacementGroup(string group)
    {
        _groupName = group;
    }

    string _groupName;
    ObservableCollection<IPlacement> _children = new ObservableCollection<IPlacement>();
}