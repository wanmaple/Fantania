using Avalonia.Controls;

namespace FantaniaLib;

public static class MessageBoxHelper
{
    public static async Task PopupMessageOkay(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.Okay, MessageBoxIcons.None);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        await msgbox.ShowDialog<ButtonResults>(top);
    }

    public static async Task<bool> PopupMessageYesNo(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.YesNo, MessageBoxIcons.None);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result == ButtonResults.Yes;
    }

    public static async Task<ButtonResults> PopupMessageYesNoCancel(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.YesNoCancel, MessageBoxIcons.None);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result;
    }

    public static async Task PopupWarningOkay(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.Okay, MessageBoxIcons.Warning);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        await msgbox.ShowDialog<ButtonResults>(top);
    }

    public static async Task<bool> PopupWarningYesNo(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.YesNo, MessageBoxIcons.Warning);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result == ButtonResults.Yes;
    }

    public static async Task<ButtonResults> PopupWarningYesNoCancel(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.YesNoCancel, MessageBoxIcons.Warning);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result;
    }

    public static async Task PopupErrorOkay(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.Okay, MessageBoxIcons.Error);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        await msgbox.ShowDialog<ButtonResults>(top);
    }

    public static async Task<bool> PopupErrorYesNo(ContentControl owner, string message, params object[] args)
    {
        var msgbox = StandardMessageBox.Create(string.Format(message, args), ButtonEnums.YesNo, MessageBoxIcons.Error);
        Window top = (Window)TopLevel.GetTopLevel(owner)!;
        var result = await msgbox.ShowDialog<ButtonResults>(top);
        return result == ButtonResults.Yes;
    }
}