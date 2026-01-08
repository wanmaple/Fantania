using Avalonia.Controls;
using Avalonia.Interactivity;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public partial class PlacementView : UserControl
{
    PlacementViewModel ViewModel => DataContext as PlacementViewModel;

    public PlacementView()
    {
        InitializeComponent();
    }

    void TreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TreeView treeview = sender as TreeView;
        UserPlacement placement = treeview.SelectedItem as UserPlacement;
        if (placement != null)
        {
            ViewModel.SelectedPlacement = placement;
        }
        else
        {
            ViewModel.SelectedPlacement = null;
        }
    }

    void ButtonAddPlacement_Click(object? sender, RoutedEventArgs e)
    {
        ScriptTemplate template = (sender as Button).DataContext as ScriptTemplate;
        var placement = new UserPlacement(template, 1);
        placement.Name = template.ClassName + "_1";
        template.Children.Add(placement);
    }
}