using System;

namespace Fantania;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class CustomSerializationAttribute : Attribute
{
    public int Version { get; private set; }

    protected CustomSerializationAttribute(int version)
    {
        Version = version;
    }
}