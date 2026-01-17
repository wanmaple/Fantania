using System;
using FantaniaLib;

namespace Fantania.ViewModels;

public class PlacementViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;
    
    private UserPlacement? _selected = null;
    public UserPlacement? SelectedPlacement
    {
        get { return _selected; }
        set
        {
            if (_selected != value)
            {
                _selected = value;
                _workspace.PlacementModule.ActivePlacement = value;
                OnPropertyChanged(nameof(SelectedPlacement));
            }
        }
    }

    public PlacementViewModel(Workspace workspace)
    {
        _workspace = workspace;
    }

    public void FilterPlacements(Predicate<IPlacement>? filter)
    {
        foreach (var group in _workspace.PlacementModule.LevelPlacements)
        {
            foreach (PlacementTemplate template in group.Source)
            {
                FilterableBindingSource<IPlacement> filterable = (FilterableBindingSource<IPlacement>)template.Children;
                filterable.Filter(filter, null);
            }
        }
    }

    Workspace _workspace;
}