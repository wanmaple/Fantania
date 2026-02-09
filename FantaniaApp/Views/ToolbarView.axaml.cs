using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public class Level2NameConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[1] is not Workspace workspace) return AvaloniaProperty.UnsetValue;
        string formatText = workspace.LocalizeString("BT_CurrentLevel");
        if (values[0] == null)
            return string.Format(formatText, "{Empty}");
        if (values[0] is not Level lv) return AvaloniaProperty.UnsetValue;
        return string.Format(formatText, lv.Name);
    }
}

public partial class ToolbarView : UserControl
{
    public ToolbarView()
    {
        InitializeComponent();
    }
}