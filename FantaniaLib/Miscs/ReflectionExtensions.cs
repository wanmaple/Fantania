using System.Reflection;

namespace FantaniaLib;

public static class ReflectionExtensions
{
    public static bool IsStaticClass(this Type type)
    {
        return type.IsClass && type.IsAbstract && type.IsSealed;
    }

    public static object? GetValueOfFieldOrProperty(this object self, string memberName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
    {
        Type type = self.GetType();
        PropertyInfo? prop = type.GetProperty(memberName, flags);
        if (prop == null || !prop.CanRead)
        {
            System.Reflection.FieldInfo? field = type.GetField(memberName, flags);
            if (field == null) return null;
            return field.GetValue(self);
        }
        return prop.GetValue(self);
    }

    public static bool IsGenericTypeOf(this object self, Type genericType)
    {
        Type type = self.GetType();
        if (!type.IsGenericType) return false;
        var genericTypeDef = type.GetGenericTypeDefinition();
        return genericTypeDef == genericType;
    }
}