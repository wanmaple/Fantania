local RenderPipelineSetup = {
	resolution = {
		x = 640,
		y = 360,
	},
	lightCullingTileSize = 64,
	frameBuffers = {
		-- Color is required.
		{
			name = "Color",
			description = {
				width = 640,
				height = 360,
				colorDescription = {
					format = TextureFormats.RGBA8,
					minFilter = TextureMinFilters.Nearest,
					magFilter = TextureMagFilters.Nearest,
					wrapS = TextureWraps.ClampToEdge,
					wrapT = TextureWraps.ClampToEdge,
				},
				depthFormat = DepthFormats.Depth24Stencil8,
			},
		},
		{
			name = "LightOccluderMask",
			description = {
				width = 640,
				height = 360,
				colorDescription = {
					format = TextureFormats.RGBA8,
					minFilter = TextureMinFilters.Nearest,
					magFilter = TextureMagFilters.Nearest,
					wrapS = TextureWraps.ClampToEdge,
					wrapT = TextureWraps.ClampToEdge,
				},
				depthFormat = DepthFormats.None,
			},
		},
		-- {
		-- 	name = "JFA1",
		-- 	description = {
		-- 		width = 320,
		-- 		height = 180,
		-- 		colorDescription = {
		-- 			format = TextureFormats.RG16F,
		-- 			minFilter = TextureMinFilters.Nearest,
		-- 			magFilter = TextureMagFilters.Nearest,
		-- 			wrapS = TextureWraps.ClampToEdge,
		-- 			wrapT = TextureWraps.ClampToEdge,
		-- 		},
		-- 		depthFormat = DepthFormats.None,
		-- 	},
		-- },
		-- {
		-- 	name = "JFA2",
		-- 	description = {
		-- 		width = 320,
		-- 		height = 180,
		-- 		colorDescription = {
		-- 			format = TextureFormats.RG16F,
		-- 			minFilter = TextureMinFilters.Nearest,
		-- 			magFilter = TextureMagFilters.Nearest,
		-- 			wrapS = TextureWraps.ClampToEdge,
		-- 			wrapT = TextureWraps.ClampToEdge,
		-- 		},
		-- 		depthFormat = DepthFormats.None,
		-- 	},
		-- },
	},
	stages = {
		BuiltinPipelineStages.TiledLightCulling,
		BuiltinPipelineStages.LightOccluderSDF,
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
			key = "LightOccluderMask",
			vertShader = BuiltinShaders.VS_StandardNoFlip,
			fragShader = BuiltinShaders.FS_LightOccluderMask,
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