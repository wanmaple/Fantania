namespace FantaniaLib;

public interface ISerializableData
{
    IReadOnlyList<FieldInfo> SerializableFields { get; }

    object? GetFieldValue(string fieldName);
    void SetFieldValue(string fieldName, object? value);
    
    string OnCopy(IWorkspace workspace);
    void OnPaste(IWorkspace workspace, string serializedData);
}