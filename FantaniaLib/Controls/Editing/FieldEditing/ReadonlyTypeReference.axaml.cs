using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FantaniaLib;

public class TypeReference2ReferenceNameConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not TypeReference typeRef) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        DatabaseObject? reference = workspace.DatabaseModule.GetTypedObject<DatabaseObject>(typeRef.ReferenceType, typeRef.ReferenceID);
        if (reference == null)
            return workspace.LocalizeString("CM_MissingReference");
        return $"{reference.Name} ({reference.GetDisplayName(workspace)})";
    }
}

public class TypeReference2ForegroundConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not TypeReference typeRef) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        DatabaseObject? reference = workspace.DatabaseModule.GetTypedObject<DatabaseObject>(typeRef.ReferenceType, typeRef.ReferenceID);
        if (reference == null)
            return Brushes.IndianRed;
        return Brushes.White;
    }
}

public partial class ReadonlyTypeReference : UserControl
{
    public ReadonlyTypeReference()
    {
        InitializeComponent();
    }
}