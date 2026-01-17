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
            if (min < 0) min = 0;
            if (max < 0) max = 0;
            if (min > max) min = max;
            Vector2Int defOffset = v.Table.Get("defaultOffset").GetObjectOrDefault(new Vector2Int(32, 0));
            return new NodeOptions
            {
                Minimum = min,
                Maximum = max,
                DefaultOffset = defOffset,
            };
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<LevelEntity>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("anchor", DynValue.FromObject(env, v.Anchor));
            ret.Table.Set("position", DynValue.FromObject(env, v.Position));
            ret.Table.Set("rotation", DynValue.FromObject(env, v.Rotation));
            ret.Table.Set("scale", DynValue.FromObject(env, v.Scale));
            ret.Table.Set("depth", DynValue.FromObject(env, v.RealDepth));
            ret.Table.Set("color", DynValue.FromObject(env, v.Color));
            ret.Table.Set("placementRef", DynValue.FromObject(env, v.PlacementReference));
            var data = DynValue.NewTable(env);
            ret.Table.Set("data", data);
            IWorkspace workspace = ((WorkspaceProxy)env.Globals["Workspace"]).RealWorkspace;
            UserPlacement placement = v.GetReferencedPlacement(workspace)!;
            foreach (var field in placement.SerializableFields)
            {
                data.Table.Set(field.FieldName.MakeFirstCharacterLower(), DynValue.FromObject(env, placement.GetFieldValue(field.FieldName)));
            }
            var nodes = DynValue.NewTable(env);
            ret.Table.Set("nodes", nodes);
            foreach (var node in v.Nodes)
            {
                nodes.Table.Append(DynValue.FromObject(env, node));
            }
            return ret;
        });
    }
}