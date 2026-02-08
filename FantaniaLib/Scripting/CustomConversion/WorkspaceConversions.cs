using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class WorkspaceConversions
{
    public static void AutoConversions()
    {
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(LevelEditConfig), v =>
        {
            int gridAlign = v.Table.Get("gridAlign").GetIntegerOrDefault(16);
            float zoomSensitivity = v.Table.Get("zoomSensitivity").GetFloatOrDefault(1.5f);
            float zoomMin = v.Table.Get("zoomMin").GetFloatOrDefault(0.1f);
            float zoomMax = v.Table.Get("zoomMax").GetFloatOrDefault(10.0f);
            return new LevelEditConfig
            {
                GridAlign = gridAlign,
                ZoomSensitivity = zoomSensitivity,
                ZoomMin = zoomMin,
                ZoomMax = zoomMax,
            };
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(NodeOptions), v =>
        {
            int min = v.Table.Get("min").GetIntegerOrDefault(0);
            int max = v.Table.Get("max").GetIntegerOrDefault(0);
            if (min < 1) min = 1;
            if (max > 0 && min > max) max = min;
            Vector2Int defOffset = v.Table.Get("defaultOffset").GetObjectOrDefault(new Vector2Int(32, 0));
            return new NodeOptions
            {
                Minimum = min,
                Maximum = max,
                DefaultOffset = defOffset,
            };
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<UserPlacement>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            foreach (var field in v.SerializableFields)
            {
                ret.Table.Set(field.FieldName, DynValue.FromObject(env, v.GetFieldValue(field.FieldName)));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<LevelEntityNode>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("position", DynValue.FromObject(env, v.Position));
            ret.Table.Set("rotation", DynValue.NewNumber(v.Rotation));
            ret.Table.Set("scale", DynValue.FromObject(env, v.Scale));
            return ret;
        });
    }
}