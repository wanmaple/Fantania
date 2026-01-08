namespace FantaniaLib;

public static class TypeExtensions
{
    public static bool IsStaticClass(this Type type)
    {
        return type.IsClass && type.IsAbstract && type.IsSealed;
    }
}