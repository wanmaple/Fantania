using System;

namespace Fantania;

/// <summary>
/// Only works on DatabaseObject, ignore the type to sync database.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IgnoreDatabaseAttribute : Attribute
{
}