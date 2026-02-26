using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public class LayerOption
{
    public int Layer { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Workspace2LayersConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        var config = workspace.ScriptingModule.GetCustomLevelEditConfigOrDefault();
        var src = new List<LayerOption>();
        for (int i = LevelModule.MAX_LAYER; i >= LevelModule.MIN_LAYER; i--)
        {
            string? name = null;
            if (!config.LayerNames.TryGetValue(i, out name) || string.IsNullOrEmpty(name))
            {
                name = $"Unnamed: {i}";
            }
            src.Add(new LayerOption { Layer = i, Name = name });
        }
        return src;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class LayerBox : UserControl
{
    public LayerBox()
    {
        InitializeComponent();
    }
}