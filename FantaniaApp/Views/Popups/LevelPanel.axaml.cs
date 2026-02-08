using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Fantania.Localization;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public class Level2ForeColorConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not LevelDescription desc) return AvaloniaProperty.UnsetValue;
        if (values[1] == null) return Brushes.White;
        if (values[1] is not Level lv) return AvaloniaProperty.UnsetValue;
        return lv.Name == desc.Name ? Brushes.LightGreen : Brushes.White;
    }
}

public class IsNotCurrentLevelConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not LevelDescription desc) return AvaloniaProperty.UnsetValue;
        if (values[1] == null) return true;
        if (values[1] is not Level lv) return AvaloniaProperty.UnsetValue;
        return lv.Name != desc.Name;
    }
}

public partial class LevelPanel : UserControl
{
    LevelViewModel ViewModel => (LevelViewModel)DataContext!;

    public LevelPanel()
    {
        InitializeComponent();
    }

    async void BtnSwitchLevel_Click(object? sender, RoutedEventArgs e)
    {
        LevelDescription desc = (LevelDescription)((Button)sender!).DataContext!;
        Workspace workspace = ViewModel.Workspace;
        if (workspace.IsModified)
        {
            var result = await MessageBoxHelper.PopupWarningYesNoCancel(AvaloniaHelper.GetTopWindow(), LocalizationHelper.GetLocalizedString("WARN_ConfirmSaveWorkspaceSwitch"));
            if (result == ButtonResults.Cancel) return;
            if (result == ButtonResults.Yes)
            {
                await workspace.Save();
                workspace.UndoStack.Clear();
            }
        }
        await workspace.LevelModule.LoadLevel(desc.Name);
        e.Handled = true;
    }

    async void TbLevel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsMiddleButtonPressed)
        {
            LevelDescription desc = (LevelDescription)((TextBlock)sender!).DataContext!;
            Workspace workspace = ViewModel.Workspace;
            if (await MessageBoxHelper.PopupWarningYesNo(AvaloniaHelper.GetTopWindow(), string.Format(LocalizationHelper.GetLocalizedString("WARN_ConfirmDeleteLevel"), desc.Name)))
            {
                workspace.UndoStack.Clear();
                await workspace.LevelModule.DeleteLevel(desc.Name);
            }
        }
        e.Handled = true;
    }
}