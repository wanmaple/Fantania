namespace FantaniaLib;

public interface ISerializableData
{
    IReadOnlyList<FieldInfo> SerializableFields { get; }

    object? GetFieldValue(string fieldName);
}