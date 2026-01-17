using System.Numerics;
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
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(IRenderableSizer), v =>
        {
            SizerTypes type = v.Table.Get("type").GetEnumOrDefault(SizerTypes.None);
            if (type == SizerTypes.Texture)
            {
                TextureDefinition def = v.Table.Get("texture").ToObject<TextureDefinition>();
                return new TextureSizer(def);
            }
            else if (type == SizerTypes.Fixed)
            {
                Vector2Int size = v.Table.Get("size").GetObjectOrDefault(Vector2Int.Zero);
                return new FixedSizer(size);
            }
            return FallbackSizer.Fallback;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(DesiredUniformMap), v =>
        {
            var uniforms = new DesiredUniformMap();
            foreach (var key in v.Table.Keys)
            {
                if (key.Type != DataType.String) continue;
                string name = key.String;
                DynValue tb = v.Table.Get(name);
                if (tb.Table.Get("type").IsNil()) continue;
                UniformTypes type = tb.Table.Get("type").GetEnumOrDefault(UniformTypes.Float1);
                DynValue val = tb.Table.Get("value");
                object value = type switch
                {
                    UniformTypes.Float1 => val.GetFloatOrDefault(0.0f),
                    UniformTypes.Float2 => val.GetObjectOrDefault(Vector2.Zero),
                    UniformTypes.Float3 => val.GetObjectOrDefault(Vector3.Zero),
                    UniformTypes.Float4 => val.GetObjectOrDefault(Vector4.Zero),
                    UniformTypes.Matrix3x3 => val.GetObjectOrDefault(Matrix3x3.Identity),
                    UniformTypes.Texture => TextureDefinition.None,
                    _ => 0.0f,
                };
                var desiredUniform = new DesiredUniformValue
                {
                    Type = type,
                    Value = value,
                };
                uniforms.SetUniform(name, desiredUniform);
            }
            return uniforms;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(ScriptRenderInfo), v =>
        {
            var stage = v.Table.Get("stage").GetStringOrDefault(string.Empty);
            int depth = v.Table.Get("depth").GetIntegerOrDefault(0);
            Vector4 color = v.Table.Get("color").GetObjectOrDefault(Vector4.One);
            string matKey = v.Table.Get("materialKey").GetStringOrDefault(string.Empty);
            DesiredUniformMap uniforms = v.Table.Get("uniforms").GetObjectOrDefault(new DesiredUniformMap());
            IRenderableSizer sizer = v.Table.Get("sizer").GetObjectOrDefault(FallbackSizer.Fallback);
            return new ScriptRenderInfo
            {
                Stage = stage,
                Depth = depth,
                Color = color,
                MaterialKey = matKey,
                Uniforms = uniforms,
                Sizer = sizer,
            };
        });
    }
}