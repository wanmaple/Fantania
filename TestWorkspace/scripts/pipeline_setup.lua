local RenderPipelineSetup = {
	resolution = {
		x = 1920,
		y = 1080,
	},
	frameBuffers = {
		-- {
		-- 	name = "Lighting",
		-- 	description = {
		-- 		width = 1920,
		-- 		height = 1080,
		-- 		colorFormat = TextureFormats.RGBA8,
		-- 		depthFormat = DepthFormats.None,
		-- 	},
		-- },
	},
	stages = {
		BuiltinPipelineStages.Opaque,
		BuiltinPipelineStages.Transparent,
	},
	materials = {
		{
			key = "Standard",
			vertShader = BuiltinShaders.VS_Standard,
			fragShader = BuiltinShaders.FS_Standard,
		},
		{
			key = "StandardCutoff",
			vertShader = BuiltinShaders.VS_Standard,
			fragShader = BuiltinShaders.FS_StandardCutoff,
		},
		{
			key = "PureColor",
			vertShader = BuiltinShaders.VS_Standard,
			fragShader = "shaders/pure_color.fs",
		},
	},
}

return RenderPipelineSetup