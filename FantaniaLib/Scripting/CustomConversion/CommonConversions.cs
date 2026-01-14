using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class CommonConversions
{
    public static void AutoConversions()
    {
        // Type
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.String, typeof(Type), v =>
        {
            string typeStr = v.String;
            return Type.GetType(typeStr);
        });
    }
}