local RenderPipelineSetup = {
	resolution = {
		x = 640,
		y = 360,
	},
	lightCullingTileSize = 64,
	frameBuffers = {
		-- {
		-- 	name = "MyTemplate.UIHud",
		-- 	description = {
		-- 		width = 1920,
		-- 		height = 1080,
		-- 		colorFormat = TextureFormats.RGBA8,
		-- 		depthFormat = DepthFormats.None,
		-- 	},
		-- },
	},
	stages = {
		BuiltinPipelineStages.TiledLightCulling,
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
			key = "StandardLit",
			vertShader = "shaders/vert_standard_lit.vs",
			fragShader = "shaders/frag_standard_lit.fs",
		},
		{
			key = "PureColor",
			vertShader = BuiltinShaders.VS_Standard,
			fragShader = "shaders/pure_color.fs",
		},
	},
}

return RenderPipelineSetup