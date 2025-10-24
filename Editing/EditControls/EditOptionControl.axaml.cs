using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Fantania;

public partial class EditOptionControl : UserControl
{
    public IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public static readonly StyledProperty<IReadOnlyDictionary<int, string>> Value2NameProperty = AvaloniaProperty.Register<EditOptionControl, IReadOnlyDictionary<int, string>>(nameof(Value2Name), defaultValue: null);
    public IReadOnlyDictionary<int, string> Value2Name
    {
        get => GetValue(Value2NameProperty);
        set => SetValue(Value2NameProperty, value);
    }

    public EditOptionControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Value2Name = (EditableProperty.EditInfo as ConstantOptionAttribute).DisplayMap;
    }
}