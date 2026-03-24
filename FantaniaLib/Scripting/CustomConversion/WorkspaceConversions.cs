using System.Numerics;
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
            DynValue table = v.Table.Get("namedLayers");
            var dict = new Dictionary<int, string>();
            foreach (var pair in table.Table.Pairs)
            {
                int layer = (int)pair.Key.Number;
                string name = pair.Value.GetStringOrDefault(string.Empty);
                dict.Add(layer, name);
            }
            return new LevelEditConfig
            {
                GridAlign = gridAlign,
                ZoomSensitivity = zoomSensitivity,
                ZoomMin = zoomMin,
                ZoomMax = zoomMax,
                LayerNames = dict,
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
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(TileInfo), v =>
        {
            string stage = v.Table.Get("stage").GetStringOrDefault(string.Empty);
            string materialKey = v.Table.Get("material").GetStringOrDefault(string.Empty);
            DesiredUniformMap uniforms = v.Table.Get("uniforms").GetObjectOrDefault(new DesiredUniformMap());
            Vector2 uvOffset = v.Table.Get("uvOffset").GetObjectOrDefault(Vector2.Zero);
            Vector2 uvSize = v.Table.Get("uvSize").GetObjectOrDefault(Vector2.One);
            Vector4 color = v.Table.Get("color").GetObjectOrDefault(Vector4.One);
            IReadOnlyDictionary<string, TextureFilters> overrideTextureFilters = v.Table.Get("overrideTextureFilters").GetObjectOrDefault(new Dictionary<string, TextureFilters>());
            return new TileInfo
            {
                RenderStage = stage,
                MaterialKey = materialKey,
                Uniforms = uniforms,
                UVOffset = uvOffset,
                UVSize = uvSize,
                Color = color,
                OverrideTextureFilters = overrideTextureFilters,
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
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<ScriptObject>((env, v) =>
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
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<ExportSettings>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            foreach (var field in v.SerializableFields)
            {
                ret.Table.Set(field.FieldName, DynValue.FromObject(env, v.GetFieldValue(field.FieldName)));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<ExportVariant>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("type", DynValue.NewNumber((int)v.Type));
            ret.Table.Set("value", ConversionHelper.FieldTypeToDynValue(env, v.Type, v.Value!));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(ExportVariant), v =>
        {
            FieldTypes type = v.Table.Get("type").GetEnumOrDefault(FieldTypes.String);
            object? value = ConversionHelper.FieldTypeToValue(type, v.Table.Get("value"));
            return new ExportVariant
            {
                Type = type,
                Value = value,
            };
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<ExportProperty>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("name", DynValue.NewString(v.Name));
            ret.Table.Set("value", DynValue.FromObject(env, v.Variant));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<ExportEntity>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("type", DynValue.NewString(v.EntityType));
            var eProps = DynValue.FromObject(env, v.EntityProperties);
            ret.Table.Set("entityProperties", eProps);
            var tProps = DynValue.FromObject(env, v.TemplateProperties);
            ret.Table.Set("templateProperties", tProps);
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(IDRemap), v =>
        {
            var name = v.Table.Get("name").String;
            var remapTable = v.Table.Get("remap").Table;
            var remap = new Dictionary<int, int>();
            foreach (var pair in remapTable.Pairs)
            {
                int key = (int)pair.Key.Number;
                int value = (int)pair.Value.Number;
                remap.Add(key, value);
            }
            return new IDRemap
            {
                Name = name,
                Remap = remap,
            };
        });
    }
}