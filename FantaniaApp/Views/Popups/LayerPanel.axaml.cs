using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public class Layer2NameConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not int layer) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        var config = workspace.ScriptingModule.GetCustomLevelEditConfigOrDefault();
        string? name = null;
        if (!config.LayerNames.TryGetValue(layer, out name) || string.IsNullOrEmpty(name))
        {
            name = $"Unnamed: {layer}";
        }
        return name;
    }
}

public partial class LayerPanel : UserControl
{
    LevelViewModel ViewModel => (LevelViewModel)DataContext!;

    public LayerPanel()
    {
        InitializeComponent();
    }

    void CheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        Avalonia.Controls.CheckBox cb = (Avalonia.Controls.CheckBox)sender!;
        bool visible = cb.IsChecked.HasValue && cb.IsChecked.Value;
        var workspace = ViewModel.Workspace;
        for (int i = LevelModule.MAX_LAYER; i >= LevelModule.MIN_LAYER; i--)
        {
            workspace.LevelModule.LayerManager.SetLayerVisible(i, visible);
        }
    }
}