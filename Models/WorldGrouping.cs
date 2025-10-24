using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class WorldGroup : ObservableObject
{
    private string _groupName = string.Empty;
    public string GroupName
    {
        get { return _groupName; }
        set
        {
            if (_groupName != value)
            {
                _groupName = value;
                OnPropertyChanged(nameof(GroupName));
            }
        }
    }

    private ObservableCollection<string> _worldNames = new ObservableCollection<string>();
    public IList<string> WorldNames => _worldNames;
}

public class WorldGrouping : ObservableObject
{
    public bool IsValid => _groups.Count > 0;

    public IList<WorldGroup> Groups => _groups;

    public bool IsTheLastWorld => _groups.Count == 1 && _groups[0].WorldNames.Count == 1;

    public bool HasGroup(string groupName)
    {
        foreach (var group in _groups)
        {
            if (group.GroupName == groupName)
                return true;
        }
        return false;
    }

    public string GetGroupName(string worldName)
    {
        foreach (var group in _groups)
        {
            if (group.WorldNames.Contains(worldName))
            {
                return group.GroupName;
            }
        }
        return null;
    }

    public void AddWorldInfo(string worldName, string worldGroup)
    {
        bool found = false;
        foreach (var group in _groups)
        {
            if (group.GroupName == worldGroup)
            {
                group.WorldNames.Add(worldName);
                found = true;
                break;
            }
        }
        if (!found)
        {
            var group = new WorldGroup();
            group.GroupName = worldGroup;
            group.WorldNames.Add(worldName);
            _groups.Add(group);
        }
        OnPropertyChanged(nameof(Groups));
    }

    public void RemoveWorldInfo(string worldName, string worldGroup)
    {
        for (int i = 0; i < _groups.Count; i++)
        {
            var group = _groups[i];
            if (group.GroupName == worldGroup)
            {
                if (group.WorldNames.Remove(worldName) && group.WorldNames.Count == 0)
                {
                    _groups.RemoveAtFast(i);
                }
                break;
            }
        }
    }

    ObservableCollection<WorldGroup> _groups = new ObservableCollection<WorldGroup>();
}