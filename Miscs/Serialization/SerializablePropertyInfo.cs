using System.Reflection;

namespace Fantania;

public class SerializablePropertyInfo
{
    public int Version { get; internal set; }
    public PropertyInfo PropertyInfo { get; internal set; }
    public IPropertySerializer PropertySerializer { get; internal set; }
}