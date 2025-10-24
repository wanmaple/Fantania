using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using Fantania.Models;

namespace Fantania;

public class StandardPropertySerializerV1 : IPropertySerializer
{
    public object Deserialize(Type propType, BinaryReader reader, Workspace workspace)
    {
        if (object.ReferenceEquals(propType, typeof(int)))
            return reader.ReadInt32();
        else if (object.ReferenceEquals(propType, typeof(double)))
            return reader.ReadDouble();
        else if (object.ReferenceEquals(propType, typeof(string)))
            return reader.ReadString();
        else if (propType.IsEnum)
        {
            int integer = reader.ReadInt32();
            string name = Enum.GetName(propType, integer);
            return Enum.Parse(propType, name);
        }
        else if (object.ReferenceEquals(propType, typeof(Avalonia.Vector)))
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            return new Avalonia.Vector(x, y);
        }
        else if (object.ReferenceEquals(propType, typeof(Vector4)))
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return new Vector4(x, y, z, w);
        }
        else if (propType.IsAssignableTo(typeof(DatabaseObject)))
        {
            string group = reader.ReadString();
            int id = reader.ReadInt32();
            return workspace.MainDatabase.ObjectsOfGroup(group).First(obj => obj.ID == id);
        }
        else
            throw new SerializationException($"Unknown type '{propType}' to deserialize.");
    }

    public void Serialize(object value, BinaryWriter writer)
    {
        Type propType = value.GetType();
        if (object.ReferenceEquals(propType, typeof(int)))
            writer.Write((int)value);
        else if (object.ReferenceEquals(propType, typeof(double)))
            writer.Write((double)value);
        else if (object.ReferenceEquals(propType, typeof(string)))
            writer.Write(value as string);
        else if (propType.IsEnum)
            writer.Write((int)value);
        else if (object.ReferenceEquals(propType, typeof(Avalonia.Vector)))
        {
            Avalonia.Vector vec = (Avalonia.Vector)value;
            writer.Write(vec.X);
            writer.Write(vec.Y);
        }
        else if (object.ReferenceEquals(propType, typeof(Vector4)))
        {
            Vector4 vec = (Vector4)value;
            writer.Write(vec.X);
            writer.Write(vec.Y);
            writer.Write(vec.Z);
            writer.Write(vec.W);
        }
        else if (propType.IsAssignableTo(typeof(DatabaseObject)))
        {
            DatabaseObject dbObj = value as DatabaseObject;
            string group = dbObj.Group;
            int id = dbObj.ID;
            writer.Write(group);
            writer.Write(id);
        }
        else
            throw new SerializationException($"Unknown type '{propType}' to serialize.");
    }
}