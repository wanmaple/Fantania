using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fantania.Models;

namespace Fantania.ViewModels;

public class StylegroundGroup : ObservableObject
{
    public bool Visible => true;
    public string Name { get; set; }
    public IReadOnlyList<Styleground> Items { get; set; }
}

public partial class StylegroundEditorViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    private Styleground _selectedSg;
    public Styleground SelectedStyleground
    {
        get { return _selectedSg; }
        set
        {
            if (_selectedSg != value)
            {
                _selectedSg = value;
                OnPropertyChanged(nameof(SelectedStyleground));
            }
        }
    }

    public ObservableCollection<IPlacement> Placements => _placements;
    public ObservableCollection<StylegroundGroup> StylegroundGroups => _groups;

    public StylegroundEditorViewModel(Workspace workspace)
    {
        _workspace = workspace;
        InitializeGroups();
        InitializePlacements();
    }

    [RelayCommand]
    void Undo()
    {
        if (_workspace != null)
        {
            _workspace.UndoStack.Undo();
        }
    }

    [RelayCommand]
    void Redo()
    {
        if (_workspace != null)
        {
            _workspace.UndoStack.Redo();
        }
    }

    void InitializeGroups()
    {
        _groups =
        [
            new StylegroundGroup
            {
                Name = Localization.Resources.TreeViewHeaderBackgrounds,
                Items = Workspace.CurrentStylegrounds.Backgrounds,
            },
            new StylegroundGroup
            {
                Name = Localization.Resources.TreeViewHeaderForegrounds,
                Items = Workspace.CurrentStylegrounds.Foregrounds,
            },
        ];
    }

    void InitializePlacements()
    {
        _placements = new ObservableCollection<IPlacement>();
        var bgs = new PlacementContainer(Localization.Resources.PlacementStylegrounds, "avares://Fantania/Assets/icons/placements/bg.png");
        // bgs.Children.Add(new PlacementGroup(Localization.Resources.PlacementGradientStylegrounds, "avares://Fantania/Assets/icons/placements/gradient.png", Localization.Resources.TooltipGradientStylegrounds, typeof(GradientStylegroundTemplate)));
        // bgs.Children.Add(new PlacementGroup(Localization.Resources.PlacementDrawablesStylegrounds, "", Localization.Resources.TooltipDrawablesStylegrounds, typeof(DrawablesStylegroundTemplate)));
        _placements.Add(bgs);
    }

    ObservableCollection<StylegroundGroup> _groups;
    ObservableCollection<IPlacement> _placements;
    Workspace _workspace;
}