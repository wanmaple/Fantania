using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public class GroupReference2OptionsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not GroupReference groupRef) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        return workspace.DatabaseModule.GetObjectsOfGroup(groupRef.ReferenceGroup);
    }
}

public class GroupReference2ReferedObjectConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not GroupReference groupRef) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        if (groupRef.ReferenceID == 0)
            return EmptyDatabaseObject.Instance;
        DatabaseObject? obj = workspace.DatabaseModule.GetGroupedObject<DatabaseObject>(groupRef.ReferenceGroup, groupRef.ReferenceID);
        return obj ?? EmptyDatabaseObject.Instance;
    }
}

public partial class GroupReferenceBox : UserControl
{
    IEditableField? Field => DataContext as IEditableField;

    public GroupReferenceBox()
    {
        InitializeComponent();
    }

    void AutoCompleteBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AutoCompleteBox acb = (AutoCompleteBox)sender!;
        GroupReference groupRef = (GroupReference)Field!.FieldValue;
        if (acb.SelectedItem != null)
        {
            DatabaseObject obj = (DatabaseObject)acb.SelectedItem;
            groupRef.ReferenceID = obj.ID;
            Field!.FieldValue = groupRef;
        }
    }
}