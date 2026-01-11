using Avalonia;
using Avalonia.Controls;

namespace FantaniaLib;

public partial class ObjectEditView : UserControl
{
    public static readonly StyledProperty<IEnumerable<GroupedEditableFields>?> EditableFieldsProperty = AvaloniaProperty.Register<ObjectEditView, IEnumerable<GroupedEditableFields>?>(nameof(EditableFields), defaultValue: null);
    public IEnumerable<GroupedEditableFields>? EditableFields
    {
        get => GetValue(EditableFieldsProperty);
        set => SetValue(EditableFieldsProperty, value);
    }

    public ObjectEditView()
    {
        InitializeComponent();
    }
}