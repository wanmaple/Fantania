using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Fantania.Localization;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class LevelViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public LevelViewModel(Workspace workspace)
    {
        _workspace = workspace;
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
                await Workspace.LogModule.LogErrorAsync(string.Format(LocalizationHelper.GetLocalizedString("ERR_CreateLevelFailure"), ex));
            }
        }
    }

    Workspace _workspace;
}