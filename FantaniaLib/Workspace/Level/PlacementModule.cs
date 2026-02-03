using System.Collections.ObjectModel;

namespace FantaniaLib;

public class PlacementModule : WorkspaceModule
{
    public PlacementTemplate? FallbackTemplate { get; internal set; }
    public IReadOnlyList<IPlacement> LevelPlacements => _levelPlacements;
    public IReadOnlyDictionary<string, PlacementTemplate> PlacementTemplateMap => _placementTemplateMap;

    public UserPlacement? ActivePlacement { get; set; }

    public PlacementModule(IWorkspace workspace) : base(workspace)
    {}

    internal void Sync()
    {
        foreach (PlacementTemplate template in _placementTemplateMap.Values)
        {
            var objs = _workspace.DatabaseModule.GetObjectsOfType(template.ClassName);
            foreach (UserPlacement placement in objs)
            {
                template.Source.Add(placement);
                _workspace.DatabaseModule.WatchPropertyChange(placement);
            }
        }
    }
    
    public void AddLevelTemplate(PlacementTemplate template)
    {
        string group = template.Group;
        IPlacement? placementGroup = _levelPlacements.FirstOrDefault(p => p.Name == group);
        if (placementGroup == null)
        {
            placementGroup = new PlacementGroup(group);
            _levelPlacements.Add(placementGroup);
        }
        placementGroup.Source.Add(template);
        _placementTemplateMap.Add(template.ClassName, template);
    }

    public UserPlacement AddUserPlacement(string templateName)
    {
        PlacementTemplate template = _placementTemplateMap[templateName];
        int id = template.Source.Count <= 0 ? 1 : template.Source.Max(p => ((UserPlacement)p).ID) + 1;
        var placement = new UserPlacement(template, id);
        placement.Name = $"SN_{template.ClassName}_{id}";
        placement.Tooltip = $"ST_{template.ClassName}_{id}";
        template.Source.Add(placement);
        _workspace.DatabaseModule.AddObject(placement);
        _workspace.DatabaseModule.WatchPropertyChange(placement);
        _workspace.UndoStack.AddOperation(new NewDatabaseObjectOperation(_workspace, placement));
        _workspace.UndoStack.AddOperation(new NewPlacementOperation(_workspace, template, placement));
        return placement;
    }

    public bool RemoveUserPlacement(UserPlacement placement)
    {
        PlacementTemplate template = _placementTemplateMap[placement.Template.ClassName];
        bool ret = template.Source.Remove(placement);
        _workspace.DatabaseModule.RemoveObject(placement);
        _workspace.DatabaseModule.UnwatchPropertyChange(placement);
        _workspace.UndoStack.AddOperation(new DelDatabaseObjectOperation(_workspace, placement));
        _workspace.UndoStack.AddOperation(new DelPlacementOperation(_workspace, template, placement));
        return ret;
    }

    Dictionary<string, PlacementTemplate> _placementTemplateMap = new Dictionary<string, PlacementTemplate>(128);
    ObservableCollection<IPlacement> _levelPlacements = new ObservableCollection<IPlacement>();
}