local RenderPipelineSetup = {
	resolution = {
		x = 1920,
		y = 1080,
	},
	frameBuffers = {
		{
			name = "Lighting",
			description = {
				width = 1920,
				height = 1080,
				colorFormat = TextureFormats.RGBA8,
				depthFormat = DepthFormats.None,
			},
		},
	},
	stages = {
		BuiltinPipelineStages.Opaque,
		BuiltinPipelineStages.Transparent,
	},
	materials = {
		{
			key = "SampleTexture",
			vertShader = BuiltinShaders.VS_Standard,
			fragShader = BuiltinShaders.FS_Sample,
		},
	},
}

return RenderPipelineSetup