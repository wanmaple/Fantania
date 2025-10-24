using System;

namespace Fantania;

/// <summary>
/// Only works on DatabaseObject
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DataGroupAttribute : Attribute
{
    public string Group { get; set; }
}