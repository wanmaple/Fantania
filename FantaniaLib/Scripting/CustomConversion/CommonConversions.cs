using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class CommonConversions
{
    public static void AutoConversions()
    {
        // Type
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Type>((env, v) =>
        {
            DynValue str = DynValue.NewString(v.FullName);
            return str;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.String, typeof(Type), v =>
        {
            string typeStr = v.String;
            return Type.GetType(typeStr);
        });
    }
}