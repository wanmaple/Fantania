using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Fantania.Models;

namespace Fantania;

public partial class NewWorldView : Window
{
    public event Action<bool, string, string> ConfirmOrCancel;

    public Workspace Workspace => DataContext as Workspace;

    public NewWorldView()
    {
        InitializeComponent();
    }

    public async Task Confirm()
    {
        string worldName = txtName.Text == null ? string.Empty : txtName.Text.Trim();
        string groupName = chkNewGroup.IsChecked.Value ? txtNewGroup.Text : (cbGroups.SelectedItem as WorldGroup).GroupName;
        if (string.IsNullOrWhiteSpace(groupName))
            groupName = "Default";
        if (string.IsNullOrEmpty(worldName))
        {
            await MessageBoxHelper.PopupErrorOkay(this, "ErrorEmptyWorldName");
            return;
        }
        Regex reg = new Regex(@"^[\w]+$");
        if (!reg.IsMatch(worldName))
        {
            await MessageBoxHelper.PopupErrorOkay(this, "ErrorInvalidWorldName");
            return;
        }
        if (Workspace.WorldGrouping.Groups.Any(group => group.WorldNames.Contains(worldName)))
        {
            await MessageBoxHelper.PopupErrorOkay(this, "ErrorDuplicateWorldName");
            return;
        }
        await Workspace.CreateWorldAsync(worldName, groupName);
        Close(true);
    }

    public void Cancel()
    {
        Close(false);
    }
}