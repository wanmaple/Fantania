using System;

namespace Fantania;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class TooltipAttribute : Attribute
{
    public string TextKey { get; private set; }
    public object[] Arguments{ get; private set; }

    public TooltipAttribute(string key, params object[] args)
    {
        TextKey = key;
        Arguments = args;
    }
}