using System;
using System.Collections.Specialized;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Fantania;

public partial class EditArrayControl : UserControl
{
    IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public EditArrayControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Type arrayType = EditableProperty.PropertyInfo.PropertyType;
        _methodAdd = arrayType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
        _methodRemoveAt = arrayType.GetMethod("RemoveAt", BindingFlags.Instance | BindingFlags.Public);
        _methodInsert = arrayType.GetMethod("Insert", BindingFlags.Instance | BindingFlags.Public);
        _methodClear = arrayType.GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public);
        if (EditableProperty.EditValue is INotifyCollectionChanged array)
        {
            array.CollectionChanged += OnCollectionChanged;
            _array = array;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (_array != null)
        {
            _array.CollectionChanged -= OnCollectionChanged;
        }
        base.OnUnloaded(e);
    }

    public void Add()
    {
        var itemType = EditableProperty.PropertyInfo.PropertyType.GetGenericArguments()[0];
        if (itemType == typeof(string))
            AddItem(string.Empty);
        else if (itemType.IsAssignableTo(typeof(DatabaseObject)))
            AddItem(null);
        else
            AddItem(Activator.CreateInstance(itemType));
        svItems.ScrollToEnd();
    }

    public void Clear()
    {
        ClearItems();
    }

    void AddItem(object item)
    {
        _methodAdd.Invoke(EditableProperty.EditValue, [item,]);
    }

    void RemoveItem(int index)
    {
        _methodRemoveAt.Invoke(EditableProperty.EditValue, [index,]);
    }

    void InsertItem(int index, object item)
    {
        _methodInsert.Invoke(EditableProperty.EditValue, [index, item,]);
    }

    void ClearItems()
    {
        _methodClear.Invoke(EditableProperty.EditValue, null);
    }

    void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var dc = DataContext;
        DataContext = null;
        DataContext = dc;
    }

    async void Rectangle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsMiddleButtonPressed)
        {
            Rectangle rect = sender as Rectangle;
            IndexedProperty prop = rect.DataContext as IndexedProperty;
            RemoveItem(prop.Index);
        }
        else if (e.Properties.IsLeftButtonPressed)
        {
            Rectangle rect = sender as Rectangle;
            IndexedProperty prop = rect.DataContext as IndexedProperty;
            await DragDrop.DoDragDrop(e, new TempDataObject(prop), DragDropEffects.Move);
        }
    }

    void Rectangle_DragEnter(object? sender, DragEventArgs e)
    {
        Rectangle rect = sender as Rectangle;
        IndexedProperty prop = rect.DataContext as IndexedProperty;
        if (prop.Index != ((e.Data as TempDataObject).Wrapped as IndexedProperty).Index)
        {
            rect.Fill = Brushes.White;
        }
    }

    void Rectangle_DragLeave(object? sender, DragEventArgs e)
    {
        Rectangle rect = sender as Rectangle;
        IndexedProperty prop = rect.DataContext as IndexedProperty;
        rect.Fill = Brushes.Gray;
    }

    void Rectangle_Drop(object? sender, DragEventArgs e)
    {
        Rectangle rect = sender as Rectangle;
        IndexedProperty to = rect.DataContext as IndexedProperty;
        IndexedProperty from = (e.Data as TempDataObject).Wrapped as IndexedProperty;
        if (from.Index < to.Index - 1)
        {
            InsertItem(to.Index, from.EditValue);
            RemoveItem(from.Index);
        }
        else if (from.Index > to.Index)
        {
            InsertItem(to.Index, from.EditValue);
            RemoveItem(from.Index + 1);
        }
        rect.Fill = Brushes.Gray;
    }

    MethodInfo _methodAdd;
    MethodInfo _methodRemoveAt;
    MethodInfo _methodInsert;
    MethodInfo _methodClear;
    INotifyCollectionChanged _array;
}