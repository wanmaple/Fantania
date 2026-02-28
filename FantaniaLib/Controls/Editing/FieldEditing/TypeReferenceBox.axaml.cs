using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public class TypeReference2OptionsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not TypeReference typeRef) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        return workspace.DatabaseModule.GetObjectsOfType(typeRef.ReferenceType);
    }
}

public class TypeReference2ReferedObjectConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not TypeReference typeRef) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        if (typeRef.ReferenceID == 0)
            return EmptyDatabaseObject.Instance;
        DatabaseObject? obj = workspace.DatabaseModule.GetTypedObject<DatabaseObject>(typeRef.ReferenceType, typeRef.ReferenceID);
        return obj ?? EmptyDatabaseObject.Instance;
    }
}

public partial class TypeReferenceBox : UserControl
{
    IEditableField? Field => DataContext as IEditableField;

    public TypeReferenceBox()
    {
        InitializeComponent();
    }

    void AutoCompleteBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AutoCompleteBox acb = (AutoCompleteBox)sender!;
        TypeReference typeRef = (TypeReference)Field!.FieldValue;
        if (acb.SelectedItem != null)
        {
            DatabaseObject obj = (DatabaseObject)acb.SelectedItem;
            typeRef.ReferenceID = obj.ID;
            Field!.FieldValue = typeRef;
        }
    }
}