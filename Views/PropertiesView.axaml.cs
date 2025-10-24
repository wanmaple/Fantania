using Avalonia;
using Avalonia.Controls;
using Fantania.ViewModels;

namespace Fantania.Views;

public partial class PropertiesView : UserControl
{
    public static readonly StyledProperty<bool> IsHeaderVisibleProperty = AvaloniaProperty.Register<PropertiesView, bool>(nameof(IsHeaderVisible), defaultValue: true);
    public bool IsHeaderVisible
    {
        get => GetValue(IsHeaderVisibleProperty);
        set => SetValue(IsHeaderVisibleProperty, value);
    }

    public PropertiesViewModel ViewModel => DataContext as PropertiesViewModel;

    public PropertiesView()
    {
        InitializeComponent();
    }
}