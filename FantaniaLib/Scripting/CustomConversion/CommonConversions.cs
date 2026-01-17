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
    }
}