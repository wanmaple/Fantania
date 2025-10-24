using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Fantania.ViewModels;

namespace Fantania;

public partial class EditCurvedEdgeControl : UserControl
{
    public static readonly StyledProperty<ObservableCollection<IEditableProperty>> EditablePropertiesProperty = AvaloniaProperty.Register<EditCurvedEdgeControl, ObservableCollection<IEditableProperty>>(nameof(EditableProperties), defaultValue: null);
    public ObservableCollection<IEditableProperty> EditableProperties
    {
        get => GetValue(EditablePropertiesProperty);
        set => SetValue(EditablePropertiesProperty, value);
    }

    public IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public EditCurvedEdgeControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        CurvedEdge edge = (CurvedEdge)EditableProperty.EditValue;
        EditableProperties = ObjectEditableCollector.CollectTemporaryEditableProperties(edge);
        pvProps.DataContext = new PropertiesViewModel(EditableProperties);
        OnEdgeChanged(edge);
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
        EditableProperty.ValueChanged += OnEdgeChanged;
    }

    void OnTick(TimeSpan dt)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var prop in EditableProperties)
            {
                object boxed = _edge;
                prop.PropertyInfo.SetValue(boxed, prop.EditValue);
                _edge = (CurvedEdge)boxed;
            }
            if (EditableProperty != null && (CurvedEdge)EditableProperty.EditValue != _edge)
            {
                EditableProperty.EditValue = _edge;
            }
        });
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    void OnEdgeChanged(object noise)
    {
        CurvedEdge edge = (CurvedEdge)noise;
        _edge = edge.Clone();
        foreach (var prop in EditableProperties)
        {
            prop.EditValue = prop.PropertyInfo.GetValue(_edge);
        }
    }

    CurvedEdge _edge;
}