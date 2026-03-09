using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FantaniaLib;

public partial class ArrayBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    private ObservableCollection<IndexedEditableField> _indexedFields = new ObservableCollection<IndexedEditableField>();
    public ObservableCollection<IndexedEditableField> IndexedFields
    {
        get { return _indexedFields; }
        set { _indexedFields = value; }
    }

    public ArrayBox()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (Field != null)
        {
            ICloneable fieldValue = (ICloneable)Field.FieldValue;
            _clone = (ICloneable)fieldValue.Clone();
            IndexedFields.Clear();
            IEnumerable list = (IEnumerable)_clone;
            int index = 0;
            foreach (object? item in list)
            {
                IndexedFields.Add(new IndexedEditableField(Field.Workspace, _clone, index, Field.EditInfo));
                index++;
            }
            if (Field.SampleInstance is INotifyPropertyChanged obj)
            {
                obj.PropertyChanged += OnInstancePropertyChanged;
            }
        }
        else
        {
            _clone = null;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        topLevel.RequestAnimationFrame(OnTick);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (Field != null)
        {
            if (Field.SampleInstance is INotifyPropertyChanged obj)
            {
                obj.PropertyChanged -= OnInstancePropertyChanged;
            }
        }
    }

    void OnInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isUpdating) return;
        if (e.PropertyName == Field!.FieldName)
        {
            ICloneable fieldValue = (ICloneable)Field.FieldValue;
            _clone = (ICloneable)fieldValue.Clone();
            IndexedFields.Clear();
            IEnumerable list = (IEnumerable)_clone;
            int index = 0;
            foreach (object? item in list)
            {
                IndexedFields.Add(new IndexedEditableField(Field.Workspace, _clone, index, Field.EditInfo));
                index++;
            }
        }
    }

    void OnTick(TimeSpan dt)
    {
        if (Field != null && _clone != null)
        {
            if (!Field.FieldValue.Equals(_clone))
            {
                _isUpdating = true;
                Field.FieldValue = _clone.Clone();
                _isUpdating = false;
            }
        }
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    public void AddItem()
    {
        if (_clone != null)
        {
            Type itemType = _clone.GetType().GetGenericArguments()[0];
            object? defaultValue = Field!.EditInfo.DefaultMemberValue;
            if (defaultValue == null)
            {
                if (itemType.IsValueType)
                {
                    defaultValue = Activator.CreateInstance(itemType);
                }
                else if (itemType == typeof(string))
                {
                    defaultValue = string.Empty;
                }
            }
            MethodInfo addMethod = _clone.GetType().GetMethod("Add")!;
            addMethod.Invoke(_clone, new object?[] { defaultValue });
            IndexedFields.Add(new IndexedEditableField(Field.Workspace, _clone, IndexedFields.Count, Field.EditInfo));
        }
    }

    public void ClearItems()
    {
        if (_clone != null)
        {
            MethodInfo clearMethod = _clone.GetType().GetMethod("Clear")!;
            clearMethod.Invoke(_clone, null);
            IndexedFields.Clear();
        }
    }

    void ButtonRemove_Click(object? sender, RoutedEventArgs e)
    {
        Button btn = (Button)sender!;
        IndexedEditableField field = (IndexedEditableField)btn.DataContext!;
        var removeMethod = _clone!.GetType().GetMethod("RemoveAt")!;
        removeMethod.Invoke(_clone, new object[] { field.Index });
        IndexedFields.Remove(field);
        for (int i = 0; i < IndexedFields.Count; i++)
        {
            IndexedFields[i].Index = i;
        }
    }

    ICloneable? _clone;
    bool _isUpdating = false;
}