using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public partial class EditFieldsView : UserControl
{
    public static readonly StyledProperty<IValueConverter> TooltipConverterProperty = AvaloniaProperty.Register<EditFieldsView, IValueConverter>(nameof(TooltipConverter), defaultValue: null);
    public IValueConverter TooltipConverter
    {
        get => GetValue(TooltipConverterProperty);
        set => SetValue(TooltipConverterProperty, value);
    }

    public EditFieldsView()
    {
        InitializeComponent();
    }
}