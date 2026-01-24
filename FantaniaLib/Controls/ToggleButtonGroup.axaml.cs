using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class RadioToggleInformation : ObservableObject
{
    public required IWorkspace Workspace { get; set; }
    public required object Value { get; set; }
    public ICommand? Command { get; set; }
    public string IconPath { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
}

public class SelectedValue2CheckedConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return false;
        if (values.Count != 2) return false;
        if (values[0] == null) return false;
        if (values[1] is not RadioToggleInformation def) return false;
        return values[0]!.Equals(def.Value);
    }
}

public class RadioToggle : ToggleButton
{
    protected override Type StyleKeyOverride => typeof(ToggleButton);

    protected override void Toggle()
    {
        if (!IsChecked.HasValue || !IsChecked.Value)
            IsChecked = true;
    }
}

public partial class ToggleButtonGroup : UserControl
{
    public static readonly StyledProperty<IEnumerable<RadioToggleInformation>?> ItemsSourceProperty = AvaloniaProperty.Register<ToggleButtonGroup, IEnumerable<RadioToggleInformation>?>(nameof(ItemsSource), defaultValue: null);
    public IEnumerable<RadioToggleInformation>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<object?> SelectedValueProperty = AvaloniaProperty.Register<ToggleButtonGroup, object?>(nameof(SelectedValue), defaultValue: null);
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public ToggleButtonGroup()
    {
        InitializeComponent();
    }

    void RadioToggle_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        RadioToggle toggle = (RadioToggle)sender!;
        if (toggle.IsChecked!.Value)
        {
            RadioToggleInformation info = (RadioToggleInformation)toggle.DataContext!;
            SelectedValue = info.Value;
        }
    }
}