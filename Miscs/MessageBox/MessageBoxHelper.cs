using System.Threading.Tasks;
using Avalonia.Controls;

namespace Fantania;

public static class MessageBoxHelper
{
    public static async Task PopupMessageOkay(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.Okay, MessageBoxIcons.None);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        await msgbox.ShowDialog<ButtonResults>(top);
    }

    public static async Task<bool> PopupMessageYesNo(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.YesNo, MessageBoxIcons.None);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result == ButtonResults.Yes;
    }

    public static async Task<ButtonResults> PopupMessageYesNoCancel(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.YesNoCancel, MessageBoxIcons.None);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result;
    }

    public static async Task PopupWarningOkay(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.Okay, MessageBoxIcons.Warning);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        await msgbox.ShowDialog<ButtonResults>(top);
    }

    public static async Task<bool> PopupWarningYesNo(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.YesNo, MessageBoxIcons.Warning);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result == ButtonResults.Yes;
    }

    public static async Task<ButtonResults> PopupWarningYesNoCancel(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.YesNoCancel, MessageBoxIcons.Warning);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result;
    }

    public static async Task PopupErrorOkay(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.Okay, MessageBoxIcons.Error);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        await msgbox.ShowDialog<ButtonResults>(top);
    }

    public static async Task<bool> PopupErrorYesNo(ContentControl owner, string messageKey, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(Localization.Resources.ResourceManager.GetString(messageKey), args), ButtonEnums.YesNo, MessageBoxIcons.Error);
        Window top = TopLevel.GetTopLevel(owner) as Window;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result == ButtonResults.Yes;
    }
}