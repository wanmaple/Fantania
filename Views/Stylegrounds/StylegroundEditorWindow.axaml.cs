using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Fantania.Views;

public partial class StylegroundEditorWindow : Window
{
    public StylegroundEditorWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Focus();
    }
}