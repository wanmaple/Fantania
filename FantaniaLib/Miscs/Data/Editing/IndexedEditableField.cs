
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class IndexedEditableField : ObservableObject, IEditableField
{
    public string FieldName => string.Empty;

    public FieldEditInfo EditInfo => throw new NotImplementedException();

    public object FieldValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Type FieldType => throw new NotImplementedException();

    public IFieldValidator? FieldValidator => throw new NotImplementedException();

    public IWorkspace Workspace => throw new NotImplementedException();
}