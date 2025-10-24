using System;
using System.IO;
using System.Reflection;
using Fantania.Models;

namespace Fantania;

public class ObjectSerializer
{
    internal ObjectSerializer(int version)
    {
        _version = version;
    }

    public void Serialize(BinaryWriter writer, object obj)
    {
        writer.Write(obj.GetType().FullName);
        var props = SerializationHelper.GetSerializableProperties(obj.GetType());
        foreach (var propInfo in props)
        {
            if (propInfo.Version > _version) continue;
            object value = propInfo.PropertyInfo.GetValue(obj);
            propInfo.PropertySerializer.Serialize(value, writer);
        }
    }

    public object Deserialize(BinaryReader reader, Workspace workspace)
    {
        string typename = reader.ReadString();
        var asm = Assembly.GetExecutingAssembly();
        Type type = asm.GetType(typename);
        var props = SerializationHelper.GetSerializableProperties(type);
        var obj = Activator.CreateInstance(type);
        foreach (var propInfo in props)
        {
            if (propInfo.Version > _version) continue;
            object value = propInfo.PropertySerializer.Deserialize(propInfo.PropertyInfo.PropertyType, reader, workspace);
            propInfo.PropertyInfo.SetValue(obj, value);
        }
        return obj;
    }

    int _version;
}