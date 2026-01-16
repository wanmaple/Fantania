using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class RenderingConversions
{
    public static void AutoConversions()
    {
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FrameBufferDescription), v =>
        {
            int width = v.Table.Get("width").GetIntegerOrDefault(1920);
            int height = v.Table.Get("height").GetIntegerOrDefault(1080);
            TextureFormats colorFormat = v.Table.Get("colorFormat").GetEnumOrDefault(TextureFormats.RGBA8);
            DepthFormats depthFormat = v.Table.Get("depthFormat").GetEnumOrDefault(DepthFormats.None);
            return new FrameBufferDescription
            {
                Width = width,
                Height = height,
                ColorFormat = colorFormat,
                DepthFormat = depthFormat,
            };
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(FrameBufferConfig), v =>
        {
            string name = v.Table.Get("name").String;
            FrameBufferDescription desc = v.Table.Get("description").ToObject<FrameBufferDescription>();
            return new FrameBufferConfig
            {
                Name = name,
                Description = desc,
            };
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(RenderPipelineConfig), v =>
        {
            Vector2Int resolution = v.Table.Get("resolution").GetObjectOrDefault(new Vector2Int(1920, 1080));
            List<FrameBufferConfig> fbCfgs = v.Table.Get("frameBuffers").GetObjectOrDefault(new List<FrameBufferConfig>(0));
            List<IPipelineStage> stages = v.Table.Get("stages").GetObjectOrDefault(new List<IPipelineStage>(0));
            List<MaterialInfo> materials = v.Table.Get("materials").GetObjectOrDefault(new List<MaterialInfo>(0));
            return new RenderPipelineConfig
            {
                Resolution = resolution,
                FrameBuffers = fbCfgs,
                Stages = stages,
                Materials = materials,
            };
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(IPipelineStage), v =>
        {
            return new ScriptablePipelineStage(v);
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(RenderState), v =>
        {
            bool depthTestEnabled = v.Table.Get("depthTestEnabled").GetBooleanOrDefault(true);
            bool depthWriteEnabled = v.Table.Get("depthWriteEnabled").GetBooleanOrDefault(false);
            bool blendEnabled = v.Table.Get("blendEnabled").GetBooleanOrDefault(true);
            BlendFuncs blendFuncSrc = v.Table.Get("blendFuncSrc").GetEnumOrDefault(BlendFuncs.SrcAlpha);
            BlendFuncs blendFuncDst = v.Table.Get("blendFuncDst").GetEnumOrDefault(BlendFuncs.OneMinusSrcAlpha);
            return new RenderState
            {
                DepthTestEnabled = depthTestEnabled,
                DepthWriteEnabled = depthWriteEnabled,
                BlendingEnabled = blendEnabled,
                BlendFuncSrc = blendFuncSrc,
                BlendFuncDst = blendFuncDst,
            };
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(MaterialInfo), v =>
        {
            string key = v.Table.Get("key").String;
            string vertShader = v.Table.Get("vertShader").String;
            string fragShader = v.Table.Get("fragShader").String;
            return new MaterialInfo
            {
                MaterialKey = key,
                VertexShader = vertShader,
                FragmentShader = fragShader,
            };
        });
    }
}