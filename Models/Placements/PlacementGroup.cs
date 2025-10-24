using System;
using System.Collections.ObjectModel;
using Fantania.ViewModels;

namespace Fantania.Models;

public class PlacementGroup : IPlacement
{
    public string Name => _name;
    public string IconPath => _iconPath;
    public string Tooltip => _tooltip;
    public ObservableCollection<IPlacement> Children => WorkspaceViewModel.Current.Workspace.MainDatabase._typedObjects[_groupType.Name];
    public Type GroupType => _groupType;

    public PlacementGroup(string name, string iconPath, string tooltip = "", Type type = null)
    {
        _name = name;
        _iconPath = iconPath;
        _tooltip = tooltip;
        _groupType = type;
    }

    string _name;
    string _iconPath;
    string _tooltip;
    Type _groupType;
}