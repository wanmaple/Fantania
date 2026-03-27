using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Fantania.Localization;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public class Placement2LocalizedConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 3) return AvaloniaProperty.UnsetValue;
        if (values[0] is not string text) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        if (values[2] is not IPlacement placement) return AvaloniaProperty.UnsetValue;
        if (placement is not UserPlacement) return workspace.LocalizeString(text);
        return text;
    }
}

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
        e.Handled = true;
    }

    async void UserPlacement_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TextBlock tb = (TextBlock)sender!;
        UserPlacement? placement = tb.DataContext as UserPlacement;
        if (placement != null)
        {
            if (e.Properties.IsMiddleButtonPressed)
            {
                var refEntities = ViewModel!.Workspace.LevelModule.GetEntitiesReferencing(placement);
                if (refEntities.Count > 0)
                {
                    if (!await MessageBoxHelper.PopupWarningYesNo(this, LocalizationHelper.GetLocalizedString("WARN_ConfirmRemovePlacementReferenced"), placement.Name))
                        return;
                }
                ViewModel!.Workspace.PlacementModule.RemoveUserPlacement(placement);
                if (refEntities.Count > 0)
                {
                    foreach (var entity in refEntities)
                    {
                        entity.RefreshSelf();
                    }
                }
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