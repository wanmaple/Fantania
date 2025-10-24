using System;
using System.IO;
using Fantania.Models;

namespace Fantania;

public interface IPropertySerializer
{
    void Serialize(object value, BinaryWriter writer);
    object Deserialize(Type propType, BinaryReader reader, Workspace workspace);
}