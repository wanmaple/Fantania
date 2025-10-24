using System;

namespace Fantania;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class DisableEditingAttribute : Attribute
{
    public string[] DisabledPropertyNames { get; private set; }

    public DisableEditingAttribute(params string[] disableProperties)
    {
        DisabledPropertyNames = disableProperties;
    }
}