using System;

namespace Fantania;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class EditAttribute : Attribute
{
    public Type ControlType { get; set; }

    protected EditAttribute()
    {
    }
}