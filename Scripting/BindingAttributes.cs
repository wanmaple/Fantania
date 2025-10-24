using System;

namespace Fantania.Scripting;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class BindingLuaAttribute : Attribute
{ }