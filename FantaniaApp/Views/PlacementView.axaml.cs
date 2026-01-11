using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public partial class PlacementView : UserControl
{
    PlacementViewModel? ViewModel => DataContext as PlacementViewModel;

    public PlacementView()
    {
        InitializeComponent();
    }

    void TreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TreeView treeview = (TreeView)sender!;
        UserPlacement? placement = treeview.SelectedItem as UserPlacement;
        if (placement != null)
        {
            ViewModel!.SelectedPlacement = placement;
        }
        else
        {
            ViewModel!.SelectedPlacement = null;
        }
    }

    void ButtonAddPlacement_Click(object? sender, RoutedEventArgs e)
    {
        Button btn = (Button)sender!;
        PlacementTemplate template = (PlacementTemplate)btn.DataContext!;
        ViewModel!.Workspace.PlacementModule.AddUserPlacement(template.ClassName);
    }

    void UserPlacement_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TextBlock tb = (TextBlock)sender!;
        UserPlacement? placement = tb.DataContext as UserPlacement;
        if (placement != null)
        {
            if (e.Properties.IsMiddleButtonPressed)
            {
                ViewModel!.Workspace.PlacementModule.RemoveUserPlacement(placement);
            }
        }
    }
}