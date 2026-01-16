using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class WorkspaceConversions
{
    public static void AutoConversions()
    {
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(LevelEditConfig), v =>
        {
            int gridAlign = v.Table.Get("gridAlign").GetIntegerOrDefault(16);
            return new LevelEditConfig
            {
                GridAlign = gridAlign,
            };
        });
    }
}