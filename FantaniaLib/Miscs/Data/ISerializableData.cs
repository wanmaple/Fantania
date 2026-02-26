namespace FantaniaLib;

public interface ISerializableData
{
    IReadOnlyList<FieldInfo> SerializableFields { get; }

    object? GetFieldValue(string fieldName);
    
    string OnCopy(IWorkspace workspace);
    void OnPaste(IWorkspace workspace, string serializedData);
}