using Avalonia;
using Avalonia.Controls;

namespace FantaniaLib;

public class Option
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public partial class OptionBox : UserControl
{
    public static readonly StyledProperty<IEnumerable<Option>?> OptionsProperty = AvaloniaProperty.Register<OptionBox, IEnumerable<Option>?>(nameof(Options), defaultValue: null);
    public IEnumerable<Option>? Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public static readonly StyledProperty<object?> ValueProperty = AvaloniaProperty.Register<OptionBox, object?>(nameof(Value), defaultValue: null);
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public OptionBox()
    {
        InitializeComponent();
    }
}