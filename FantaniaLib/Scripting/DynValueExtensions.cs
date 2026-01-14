using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class DynValueExtensions
{
    public static bool GetBooleanOrDefault(this DynValue self, bool defVal)
    {
        return self.IsNil() ? defVal : self.Boolean;
    }

    public static int GetIntegerOrDefault(this DynValue self, int defVal)
    {
        return self.IsNil() ? defVal : (int)self.Number;
    }

    public static float GetFloatOrDefault(this DynValue self, float defVal)
    {
        return self.IsNil() ? defVal : (float)self.Number;
    }

    public static string GetStringOrDefault(this DynValue self, string defVal)
    {
        return self.IsNil() ? defVal : self.String;
    }

    public static T GetObjectOrDefault<T>(this DynValue self, T defVal)
    {
        return self.IsNil() ? defVal : self.ToObject<T>();
    }

    public static T GetEnumOrDefault<T>(this DynValue self, T defVal) where T : Enum
    {
        return self.IsNil() ? defVal : (T)(object)(int)self.Number;
    }
}