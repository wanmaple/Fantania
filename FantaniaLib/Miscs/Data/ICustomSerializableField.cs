namespace FantaniaLib;

public interface ICustomSerializableField
{
    string SerializeToString(object instance);
    void DeserializeFromString(string data, object instance);
}