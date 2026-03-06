using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Fantania.Localization;
using Fantania.Models;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class LevelViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public IReadOnlyList<RadioToggleInformation> PlacementModesDefinitions => _groupPlaceModes;
    public IReadOnlyList<RadioToggleInformation> TransformModesDefinitions => _groupTransformModes;

    public PopupStates PopupStates => _popupStates;

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
        ]);
        _transModeCmd = new RelayCommand<object>(ToggleTransformMode);
        _groupTransformModes.AddRange([
            new RadioToggleInformation{
                Workspace = _workspace,
                Value = TransformGizmoTypes.None,
                Command = _transModeCmd,
                IconPath = "avares://Fantania/Assets/icons/cursor.png",
                Tooltip = "TT_CursorMode",
            },
            new RadioToggleInformation{
                Workspace = _workspace,
                Value = TransformGizmoTypes.Translation,
                Command = _transModeCmd,
                IconPath = "avares://Fantania/Assets/icons/move.png",
                Tooltip = "TT_TranslateMode",
            },
            new RadioToggleInformation{
                Workspace = _workspace,
                Value = TransformGizmoTypes.Rotation,
                Command = _transModeCmd,
                IconPath = "avares://Fantania/Assets/icons/rotate.png",
                Tooltip = "TT_RotateMode",
            },
            new RadioToggleInformation{
                Workspace = _workspace,
                Value = TransformGizmoTypes.Scale,
                Command = _transModeCmd,
                IconPath = "avares://Fantania/Assets/icons/scale.png",
                Tooltip = "TT_ScaleMode",
            },
        ]);
    }

    void TogglePlacementMode(object? value)
    {
        Workspace.EditorModule.CurrentPlacementMode = (EntityPlacementModes)value!;
    }

    void ToggleTransformMode(object? value)
    {
        Workspace.EditorModule.CurrentTransformMode = (TransformGizmoTypes)value!;
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

    [RelayCommand]
    public void ToggleLevelPopup()
    {
        PopupStates.LevelPopupIsOpen = !PopupStates.LevelPopupIsOpen;
    }

    [RelayCommand]
    public void ToggleLayerPopup()
    {
        PopupStates.LayerPopupIsOpen = !PopupStates.LayerPopupIsOpen;
    }

    [RelayCommand]
    public void ToggleLevelMetaPopup()
    {
        PopupStates.LevelMetaPopupIsOpen = !PopupStates.LevelMetaPopupIsOpen;
    }

    [RelayCommand]
    public void ToggleRenderStatisticsPopup()
    {
        PopupStates.RenderStatisticsPopupIsOpen = !PopupStates.RenderStatisticsPopupIsOpen;
    }

    [RelayCommand]
    public async Task Cut()
    {
        var selections = Workspace!.EditorModule.SelectedObjects;
        if (selections.Count == 0) return;
        var entities = SelectionHelper.GetSelectedEntities(selections);
        if (entities.Count > 0)
            await Workspace.LevelModule.CutEntities(entities);
    }

    [RelayCommand]
    public async Task Copy()
    {
        var selections = Workspace!.EditorModule.SelectedObjects;
        if (selections.Count == 0) return;
        var entities = SelectionHelper.GetSelectedEntities(selections);
        if (entities.Count > 0)
            await Workspace.LevelModule.CopyEntities(entities);
    }

    [RelayCommand]
    public async Task Paste()
    {
        Vector2Int worldPos = Workspace!.EditorModule.MouseWorldPosition;
        await Workspace.LevelModule.PasteEntities(worldPos);
    }

    Workspace _workspace;
    List<RadioToggleInformation> _groupPlaceModes = new List<RadioToggleInformation>(3);
    List<RadioToggleInformation> _groupTransformModes = new List<RadioToggleInformation>(4);
    ICommand? _placeModeCmd, _transModeCmd;
    PopupStates _popupStates = new PopupStates();
}