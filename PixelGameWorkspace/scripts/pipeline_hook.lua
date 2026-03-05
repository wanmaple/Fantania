local PipelineHook = {
    uniforms = {
        {
            name = "u_LightOccluderSDF",
            type = PipelineHookUniformTypes.FrameBufferColorAttachment0,
            value = "LightOccluderMask",
        },
    }
}

return PipelineHook