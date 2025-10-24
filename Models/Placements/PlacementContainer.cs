using System;
using System.Collections.ObjectModel;

namespace Fantania.Models;

public class PlacementContainer : IPlacement
{
    public string Name => _name;
    public string IconPath => _iconPath;
    public string Tooltip => string.Empty;
    public ObservableCollection<IPlacement> Children => _children;
    public Type GroupType => null;

    public PlacementContainer(string name, string iconPath)
    {
        _name = name;
        _iconPath = iconPath;
    }

    string _name;
    string _iconPath;
    ObservableCollection<IPlacement> _children = new ObservableCollection<IPlacement>();
}