using System.Collections.ObjectModel;

namespace FantaniaLib;

public class PlacementModule
{
    public IReadOnlyList<IPlacement> LevelPlacements => _levelPlacements;
    
    public void AddLevelTemplate(ScriptTemplate template)
    {
        string group = template.Group;
        IPlacement placementGroup = _levelPlacements.FirstOrDefault(p => p.Name == group);
        if (placementGroup == null)
        {
            placementGroup = new PlacementGroup(group);
            _levelPlacements.Add(placementGroup);
        }
        placementGroup.Children.Add(template);
        _levelTemplateMap.Add(template.ClassName, template);
    }

    Dictionary<string, ScriptTemplate> _levelTemplateMap = new Dictionary<string, ScriptTemplate>(128);
    ObservableCollection<IPlacement> _levelPlacements = new ObservableCollection<IPlacement>();
}