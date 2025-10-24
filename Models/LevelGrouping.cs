using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class LevelGroup : ObservableObject
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

    private ObservableCollection<string> _lvNames = new ObservableCollection<string>();
    public IList<string> LevelNames => _lvNames;
}

public class LevelGrouping : ObservableObject
{
    public bool IsValid => _groups.Count > 0;

    public IList<LevelGroup> Groups => _groups;

    public bool IsTheLastLevel => _groups.Count == 1 && _groups[0].LevelNames.Count == 1;

    public bool HasGroup(string groupName)
    {
        foreach (var group in _groups)
        {
            if (group.GroupName == groupName)
                return true;
        }
        return false;
    }

    public string GetGroupName(string lvName)
    {
        foreach (var group in _groups)
        {
            if (group.LevelNames.Contains(lvName))
            {
                return group.GroupName;
            }
        }
        return null;
    }

    public void AddLevelInfo(string lvName, string lvGroup)
    {
        bool found = false;
        foreach (var group in _groups)
        {
            if (group.GroupName == lvGroup)
            {
                group.LevelNames.Add(lvName);
                found = true;
                break;
            }
        }
        if (!found)
        {
            var group = new LevelGroup();
            group.GroupName = lvGroup;
            group.LevelNames.Add(lvName);
            _groups.Add(group);
        }
        OnPropertyChanged(nameof(Groups));
    }

    public void RemoveLevelInfo(string lvName, string lvGroup)
    {
        for (int i = 0; i < _groups.Count; i++)
        {
            var group = _groups[i];
            if (group.GroupName == lvGroup)
            {
                if (group.LevelNames.Remove(lvName) && group.LevelNames.Count == 0)
                {
                    _groups.RemoveAtFast(i);
                }
                break;
            }
        }
    }

    ObservableCollection<LevelGroup> _groups = new ObservableCollection<LevelGroup>();
}