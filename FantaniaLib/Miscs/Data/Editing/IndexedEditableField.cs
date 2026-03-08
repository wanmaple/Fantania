using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class IndexedEditableField : ObservableObject, IEditableField
{
    public string FieldName => $"{_indexer.Name}[{_cacheArgs[0]}]";
    public FieldEditInfo EditInfo => _editInfo;
    public object FieldValue
    {
        get => _indexer.GetValue(_array, _cacheArgs)!;
        set
        {
            if (!FieldValue.Equals(value))
            {
                if (FieldValidator == null || FieldValidator.ValidateField(Workspace, value))
                {
                    _indexer.SetValue(_array, value, _cacheArgs);
                    OnPropertyChanged(nameof(FieldValue));
                }
            }
            else if (FieldValidator != null)
                FieldValidator.ValidateField(Workspace, value);
        }
    }
    public Type FieldType => FieldValue.GetType();
    public IFieldValidator? FieldValidator => _validator;
    public object SampleInstance => _array;
    public IWorkspace Workspace => _workspace;

    public int Index
    {
        get => (int)_cacheArgs[0]!;
        set => _cacheArgs[0] = value;
    }

    public IndexedEditableField(IWorkspace workspace, object array, int index, FieldEditInfo editInfo)
    {
        _workspace = workspace;
        _array = array;
        _indexer = array.GetType().GetProperty("Item") ?? throw new InvalidOperationException("Array type must have an indexer.");
        _cacheArgs[0] = index;
        _editInfo.EditGroup = string.Empty;
        _editInfo.Tooltip = string.Empty;
        _editInfo.EditControlType = editInfo.EditControlType;
        _editInfo.EditParameter = editInfo.EditParameter;
        _editInfo.DefaultMemberValue = editInfo.DefaultMemberValue;
        _validator = EmptyFieldValidator.Empty;
        _validator.ValidateField(Workspace, FieldValue);
    }

    IWorkspace _workspace;
    object _array;
    PropertyInfo _indexer;
    object[] _cacheArgs = new object[1];
    FieldEditInfo _editInfo = new FieldEditInfo();
    IFieldValidator? _validator;
}