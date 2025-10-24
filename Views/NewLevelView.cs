using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Fantania.Models;

namespace Fantania;

public partial class NewLevelView : Window
{
    public event Action<bool, string, string> ConfirmOrCancel;

    public Workspace Workspace => DataContext as Workspace;

    public NewLevelView()
    {
        InitializeComponent();
    }

    public async Task Confirm()
    {
        string lvName = txtName.Text == null ? string.Empty : txtName.Text.Trim();
        string groupName = chkNewGroup.IsChecked.Value ? txtNewGroup.Text : (cbGroups.SelectedItem as LevelGroup).GroupName;
        if (string.IsNullOrWhiteSpace(groupName))
            groupName = "Default";
        if (string.IsNullOrEmpty(lvName))
        {
            await MessageBoxHelper.PopupErrorOkay(this, "ErrorEmptyLevelName");
            return;
        }
        Regex reg = new Regex(@"^[\w]+$");
        if (!reg.IsMatch(lvName))
        {
            await MessageBoxHelper.PopupErrorOkay(this, "ErrorInvalidLevelName");
            return;
        }
        if (Workspace.LevelGrouping.Groups.Any(group => group.LevelNames.Contains(lvName)))
        {
            await MessageBoxHelper.PopupErrorOkay(this, "ErrorDuplicateLevelName");
            return;
        }
        await Workspace.CreateLevelAsync(lvName, groupName);
        Close(true);
    }

    public void Cancel()
    {
        Close(false);
    }
}