using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Fantania.Controls;

public partial class StandardMessageBox : Window
{
    public static StandardMessageBox Create(string message, ButtonEnums buttons, MessageBoxIcons icon)
    {
        var msgbox = new StandardMessageBox();
        msgbox.DataContext = new MessageBoxSettings
        {
            Message = message,
            Buttons = buttons,
            Icon = icon,
        };
        return msgbox;
    }

    public StandardMessageBox()
    {
        InitializeComponent();
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
        Close((sender as Button).DataContext);
    }
}