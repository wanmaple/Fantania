local PipelineHook = {
    uniforms = {
        {
            name = "u_LightOccluderMask",
            type = PipelineHookUniformTypes.FrameBufferColorAttachment0,
            value = "LightOccluderMask",
        },
        -- {
        --     name = "u_LightOccluderSDF",
        --     type = PipelineHookUniformTypes.FrameBufferColorAttachment0,
        --     value = "JFA2",
        -- },
        -- {
        --     name = "u_SDFResolution",
        --     type = PipelineHookUniformTypes.Float4,
        --     value = { x = 320.0, y = 180.0, z = 1.0 / 320.0, w = 1.0 / 180.0, },
        -- },
        {
            name = "u_LightLayerDepth",
            type = PipelineHookUniformTypes.Float4,
            value = { x = 0.0, y = 80.0, z = 160.0, w = 240.0, },
            export = true,
        },
        -- {
        --     name = "u_LightLayerDepths",
        --     type = PipelineHookUniformTypes.Float4Array,
        --     value = {
        --         { x = 0.0, y = 80.0, z = 160.0, w = 240.0, },
        --         { x = 320.0, y = 400.0, z = 480.0, w = 560.0, },
        --     },
        -- },
        {
            name = "u_ShadowArguments",
            type = PipelineHookUniformTypes.Float4,
            value = { x = 128.0, y = 384.0, z = 0.0, w = 0.0, },
            export = true,
        },
    }
}

return PipelineHook