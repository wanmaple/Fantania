using System.Numerics;
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
        // Field Types
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Direction3D>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("azimuth", DynValue.FromObject(env, v.Azimuth));
            ret.Table.Set("elevation", DynValue.FromObject(env, v.Elevation));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Direction3D), v =>
        {
            float azimuth = v.Table.Get("azimuth").GetFloatOrDefault(0.0f);
            float elevation = v.Table.Get("elevation").GetFloatOrDefault(0.0f);
            return new Direction3D
            {
                Azimuth = azimuth,
                Elevation = elevation,
            };
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<TextureDefinition>((env, v) =>
        {
            int texType = (int)v.TextureType;
            DynValue ret = DynValue.NewTable(env);
            ret.Table.Set("texType", DynValue.FromObject(env, texType));
            if (v.TextureType != TextureTypes.None)
            {
                DynValue param = DynValue.NewTable(env);
                switch (v.TextureType)
                {
                    case TextureTypes.Image:
                        param.Table.Set("path", DynValue.FromObject(env, v.TextureParameters.ImageParams.ImagePath));
                        break;
                    case TextureTypes.Atlas:
                        param.Table.Set("path", DynValue.FromObject(env, v.TextureParameters.AtlasParams.AtlasPath));
                        param.Table.Set("key", DynValue.FromObject(env, v.TextureParameters.AtlasParams.FrameKey));
                        break;
                    default:
                        break;
                }
                ret.Table.Set("texParam", param);
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(TextureDefinition), v =>
        {
            TextureTypes texType = v.Table.Get("texType").GetEnumOrDefault(TextureTypes.None);
            TextureDefinition texDef = new TextureDefinition
            {
                TextureType = texType,
            };
            if (texType != TextureTypes.None)
            {
                DynValue param = v.Table.Get("texParam");
                switch (texType)
                {
                    case TextureTypes.Image:
                        string imgPath = param.Table.Get("path").GetStringOrDefault(string.Empty);
                        texDef.TextureParameters = new TextureParameterUnion
                        {
                            ImageParams = new ImageParameter
                            {
                                ImagePath = imgPath,
                            },
                        };
                        break;
                    case TextureTypes.Atlas:
                        string atlasPath = param.Table.Get("path").GetStringOrDefault(string.Empty);
                        string key = param.Table.Get("key").GetStringOrDefault(string.Empty);
                        texDef.TextureParameters = new TextureParameterUnion
                        {
                            AtlasParams = new AtlasParameter
                            {
                                AtlasPath = atlasPath,
                                FrameKey = key,
                            },
                        };
                        break;
                    default:
                        break;
                }
            }
            return texDef;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<TypeReference>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("type", DynValue.FromObject(env, v.ReferenceType));
            ret.Table.Set("id", DynValue.FromObject(env, v.ReferenceID));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(TypeReference), v =>
        {
            string type = v.Table.Get("type").GetStringOrDefault(string.Empty);
            int id = v.Table.Get("id").GetIntegerOrDefault(0);
            return new TypeReference
            {
                ReferenceType = type,
                ReferenceID = id,
            };
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<GroupReference>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            ret.Table.Set("group", DynValue.FromObject(env, v.ReferenceGroup));
            ret.Table.Set("id", DynValue.FromObject(env, v.ReferenceID));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(GroupReference), v =>
        {
            string type = v.Table.Get("group").GetStringOrDefault(string.Empty);
            int id = v.Table.Get("id").GetIntegerOrDefault(0);
            return new GroupReference
            {
                ReferenceGroup = type,
                ReferenceID = id,
            };
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<bool>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.NewBoolean(v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<bool>), v =>
        {
            var ret = new FantaniaArray<bool>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetBooleanOrDefault(false));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<int>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.NewNumber((int)v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<int>), v =>
        {
            var ret = new FantaniaArray<int>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetIntegerOrDefault(0));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<float>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.NewNumber((float)v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<float>), v =>
        {
            var ret = new FantaniaArray<float>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetFloatOrDefault(0.0f));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<string>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.NewString(v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<string>), v =>
        {
            var ret = new FantaniaArray<string>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetStringOrDefault(string.Empty));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<Vector2>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.FromObject(env, v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<Vector2>), v =>
        {
            var ret = new FantaniaArray<Vector2>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetObjectOrDefault(Vector2.Zero));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<Vector2Int>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.FromObject(env, v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<Vector2Int>), v =>
        {
            var ret = new FantaniaArray<Vector2Int>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetObjectOrDefault(Vector2Int.Zero));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<Vector3>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.FromObject(env, v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<Vector3>), v =>
        {
            var ret = new FantaniaArray<Vector3>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetObjectOrDefault(Vector3.Zero));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<Vector4>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.FromObject(env, v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<Vector4>), v =>
        {
            var ret = new FantaniaArray<Vector4>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetObjectOrDefault(Vector4.One));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<TextureDefinition>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.FromObject(env, v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<TextureDefinition>), v =>
        {
            var ret = new FantaniaArray<TextureDefinition>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetObjectOrDefault(TextureDefinition.None));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<GroupReference>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.FromObject(env, v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<GroupReference>), v =>
        {
            var ret = new FantaniaArray<GroupReference>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetObjectOrDefault(GroupReference.None));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<FantaniaArray<TypeReference>>((env, v) =>
        {
            var ret = DynValue.NewTable(env);
            for (int i = 0; i < v.Count; i++)
            {
                ret.Table.Append(DynValue.FromObject(env, v[i]));
            }
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FantaniaArray<TypeReference>), v =>
        {
            var ret = new FantaniaArray<TypeReference>();
            foreach (DynValue val in v.Table.Values)
            {
                ret.Add(val.GetObjectOrDefault(TypeReference.None));
            }
            return ret;
        });
    }
}