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
        var placement = ViewModel!.Workspace.PlacementModule.AddUserPlacement(template.ClassName);
        tvPlacements.SelectedItem = placement;
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

    void FilterBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var selected = tvPlacements.SelectedItem;
        string filterContent = txtFilter.Text!.Trim();
        ViewModel!.FilterPlacements(p => string.IsNullOrEmpty(filterContent) || p.Name.Contains(filterContent, System.StringComparison.OrdinalIgnoreCase));
        if (selected is UserPlacement placement && !string.IsNullOrEmpty(filterContent) && !placement.Name.Contains(filterContent, System.StringComparison.OrdinalIgnoreCase))
            tvPlacements.SelectedItem = null;
        else
            tvPlacements.SelectedItem = selected;
    }
}