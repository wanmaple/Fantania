
namespace FantaniaLib;

public class LightOccluderSDFStage : IPipelineStage
{
    public string Name => "Light Occluder SDF";

    public int Order => 1500;

    public void PostRender(IRenderContext context)
    {
    }

    public void PreRender(IRenderContext context)
    {
        context.CommandBuffer.SetupState(_state);
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables, Camera2DFrameData camData)
    {
        if (_meshFullScreen == null || _matSeed == null || _matBuildSDF == null || _fbJFA == null)
            return;
        context.CommandBuffer.SetRenderTarget(ConfigurableRenderPipeline.LIGHT_OCCLUDER_MASK_BUFFER);
        // LightOccluderMask 使用RGBA四层遮挡掩码，通道非零表示该层有遮挡。
        context.CommandBuffer.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        context.CommandBuffer.ClearBufferBits(BufferBits.Color);
        var list = renderables.ToList();
        list.StableSort(RenderableInverseDepthComparer.Instance);
        var groupDict = new Dictionary<(VertexDescriptor, RenderMaterial), (List<Mesh>, RenderMaterial)>();
        foreach (var renderable in list)
        {
            var key = (renderable.Mesh.Descriptor.VertexDescriptor, renderable.Material);
            if (!groupDict.TryGetValue(key, out var drawInfo))
            {
                drawInfo = (new List<Mesh>(), renderable.Material);
                groupDict.Add(key, drawInfo);
            }
            drawInfo.Item1.Add(renderable.Mesh);
        }
        foreach (var pair in groupDict)
        {
            context.CommandBuffer.Draw(pair.Value.Item1, pair.Value.Item2);
        }
        // // 生成SDF的流程：首先从LightOccluderMask生成一个初始的JFA纹理（Seed），然后进行多次Jump Flood迭代，最后根据JFA结果和LightOccluderMask生成最终的SDF。
        // FrameBuffer fbMask = context.GetFrameBuffer(ConfigurableRenderPipeline.LIGHT_OCCLUDER_MASK_BUFFER)!;
        // int jfaWidth = _fbJFA.Ping.Description.Width;
        // int jfaHeight = _fbJFA.Ping.Description.Height;
        // float maxDistancePixels = MathF.Sqrt(fbMask.Description.Width * fbMask.Description.Width + fbMask.Description.Height * fbMask.Description.Height);
        // // 第一步：Seed
        // context.CommandBuffer.SetRenderTarget(_fbJFA.PingName);
        // _matSeed.MutableUniforms.SetUniform("u_LightOccluderMask", TextureDefinition.CreateGpuDefinition(fbMask.ColorAttachment), 0);
        // context.CommandBuffer.Draw([_meshFullScreen], _matSeed);
        // // 第二步：Jump Flood 迭代 (JFA1 <-> JFA2)
        // int jump = MathHelper.NPOT(Math.Max(jfaWidth, jfaHeight));
        // while (jump >= 1)
        // {
        //     FrameBuffer src = _fbJFA.Ping;
        //     string dstBufferName = _fbJFA.PongName;
        //     var matJFA = context.MaterialSet.GetTemporaryMaterial("#FantaniaSDFJump");
        //     matJFA.MutableUniforms.SetUniform("u_JfaPrev", TextureDefinition.CreateGpuDefinition(src.ColorAttachment), 0);
        //     matJFA.MutableUniforms.SetUniform("u_JumpPixels", (float)jump);
        //     context.CommandBuffer.SetRenderTarget(dstBufferName);
        //     context.CommandBuffer.Draw([_meshFullScreen], matJFA);
        //     _fbJFA.Swap();
        //     jump >>= 1;
        //     matJFA = new RenderMaterial(matJFA.Shader);
        // }
        // // 第三步：Build 最终 SDF（仅输出 SignedDistance）
        // FrameBuffer fbNearest = _fbJFA.Ping;
        // string finalBufferName = _fbJFA.PongName;
        // FrameBuffer fbFinal = _fbJFA.Pong;
        // _matBuildSDF.MutableUniforms.SetUniform("u_LightOccluderMask", TextureDefinition.CreateGpuDefinition(fbMask.ColorAttachment), 0);
        // _matBuildSDF.MutableUniforms.SetUniform("u_JfaNearest", TextureDefinition.CreateGpuDefinition(fbNearest.ColorAttachment), 1);
        // _matBuildSDF.MutableUniforms.SetUniform("u_MaxDistancePixels", maxDistancePixels);
        // context.CommandBuffer.SetRenderTarget(finalBufferName);
        // context.CommandBuffer.Draw([_meshFullScreen], _matBuildSDF);
        // int slot = context.GlobalUniforms["u_LightOccluderSDF"].Get<UniformSet.TextureInformation>().TextureSlot;
        // context.GlobalUniforms.SetUniform("u_LightOccluderSDF", TextureDefinition.CreateGpuDefinition(fbFinal.ColorAttachment), slot);
    }

    public void Setup(IRenderContext context)
    {
        if (_meshFullScreen == null)
        {
            _meshFullScreen = MeshBuilder.CreateScreenQuad();
        }
        if (_matSeed == null)
        {
            _matSeed = context.MaterialSet.GetTemporaryMaterial("#FantaniaSDFSeed");
        }
        if (_matBuildSDF == null)
        {
            _matBuildSDF = context.MaterialSet.GetTemporaryMaterial("#FantaniaSDFBuild");
        }
        FrameBuffer fbJFA1 = context.GetFrameBuffer(ConfigurableRenderPipeline.JFA1_BUFFER)!;
        FrameBuffer fbJFA2 = context.GetFrameBuffer(ConfigurableRenderPipeline.JFA2_BUFFER)!;
        _fbJFA = new FrameBufferPingPong(ConfigurableRenderPipeline.JFA1_BUFFER, fbJFA1, ConfigurableRenderPipeline.JFA2_BUFFER, fbJFA2);
    }

    Mesh? _meshFullScreen;
    RenderMaterial? _matSeed;
    RenderMaterial? _matBuildSDF;
    FrameBufferPingPong? _fbJFA;

    readonly RenderState _state = new RenderState
    {
        DepthTestEnabled = false,
        DepthWriteEnabled = false,
        BlendingEnabled = true,
        BlendSrcFactor = BlendFuncs.One,
        BlendDstFactor = BlendFuncs.One,
    };
}