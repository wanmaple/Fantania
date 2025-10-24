using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania.Views;

public partial class StylegroundPanel : UserControl
{
    public Workspace Workspace => DataContext as Workspace;
    
    public StylegroundPanel()
    {
        InitializeComponent();
    }

    public async Task OpenEditor()
    {
        Workspace.PanelStates.HideAll();
        var window = new StylegroundEditorWindow();
        var vm = new StylegroundEditorViewModel(Workspace);
        window.DataContext = vm;
        var desktop = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        await window.ShowDialog(desktop.MainWindow);
    }
}