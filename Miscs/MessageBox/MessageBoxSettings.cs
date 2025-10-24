using System;

namespace Fantania;

[Flags]
public enum ButtonResults : uint
{
    Okay = 0x1,
    Yes = 0x2,
    No = 0x4,
    Cancel = 0x8,
}

public enum ButtonEnums : uint
{
    Okay = ButtonResults.Okay,
    YesNo = ButtonResults.Yes | ButtonResults.No,
    YesNoCancel = ButtonResults.Yes | ButtonResults.No | ButtonResults.Cancel,
}

public enum MessageBoxIcons
{
    None,
    Warning,
    Error,
}

public class MessageBoxSettings
{
    public string Message { get; set; } = string.Empty;
    public ButtonEnums Buttons { get; set; } = ButtonEnums.Okay;
    public MessageBoxIcons Icon { get; set; } = MessageBoxIcons.None;
}