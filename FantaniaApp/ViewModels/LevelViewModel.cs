using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Fantania.Localization;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class LevelViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public IReadOnlyList<RadioToggleInformation> PlacementModesDefinitions => _groupPlaceModes;

    public LevelViewModel(Workspace workspace)
    {
        _workspace = workspace;
        InitializeButtonGroupData();
    }

    void InitializeButtonGroupData()
    {
        _placeModeCmd = new RelayCommand<object>(TogglePlacementMode);
        _groupPlaceModes.AddRange([
            new RadioToggleInformation{
                Workspace = _workspace,
                Value = EntityPlacementModes.Select,
                Command = _placeModeCmd,
                IconPath = "avares://Fantania/Assets/icons/select.png",
                Tooltip = "TT_SelectMode",
            },
            new RadioToggleInformation{
                Workspace = _workspace,
                Value = EntityPlacementModes.Place,
                Command = _placeModeCmd,
                IconPath = "avares://Fantania/Assets/icons/place.png",
                Tooltip = "TT_PlaceMode",
            },
            // new RadioToggleInformation{
            //     Workspace = _workspace,
            //     Value = EntityPlacementModes.DrawRect,
            //     Command = _placeModeCmd,
            //     IconPath = "avares://Fantania/Assets/icons/drawrect.png",
            //     Tooltip = "TT_DrawRectMode",
            // },
        ]);
    }

    void TogglePlacementMode(object? value)
    {
        Workspace.EditorModule.CurrentPlacementMode = (EntityPlacementModes)value!;
    }

    [RelayCommand]
    public async Task CreateNewLevel()
    {
        var config = new LevelCreateConfig();
        var winEdit = new CommonEditObjectView();
        winEdit.Title = Resources.H_CreateLevel;
        winEdit.DataContext = new CommonEditObjectViewModel(Workspace, config);
        bool confirm = await winEdit.ShowDialog<bool>(AvaloniaHelper.GetTopWindow());
        if (confirm)
        {
            try
            {
                await Workspace.LevelModule.CreateLevel(config);
            }
            catch (Exception ex)
            {
                await Workspace.LogErrorAsync(string.Format(LocalizationHelper.GetLocalizedString("ERR_CreateLevelFailure"), ex));
            }
        }
    }

    Workspace _workspace;
    List<RadioToggleInformation> _groupPlaceModes = new List<RadioToggleInformation>(3);
    ICommand? _placeModeCmd;
}