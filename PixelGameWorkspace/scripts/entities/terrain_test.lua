local TerrainTest = Class("TerrainTest")
local Helper = require("helper_functions")

TerrainTest.group = "SG_Decoration"
TerrainTest.name = "SN_TerrainTest"
TerrainTest.tooltip = "ST_TerrainTest"
TerrainTest.dataDefs = {
    texture = {
        type = FieldTypes.Texture,
    },
    brightnessNoise = {
        type = FieldTypes.Texture,
    },
    crackNoise = {
        type = FieldTypes.Texture,
    },
    color = {
        type = FieldTypes.Color,
        default = "#363636",
    },
    cutoff = {
        type = FieldTypes.Float,
        default = 0.1,
    },
    luminanceThreshold = {
        type = FieldTypes.Float,
        default = 0.1,
    },
    brightnessScale = {
        type = FieldTypes.Float,
        default = 0.01,
    },
    brightnessStrength = {
        type = FieldTypes.Float,
        default = 1.05,
    },
    brightnessQuantizationSteps = {
        type = FieldTypes.Integer,
        default = 10,
    },
    brightnessColor = {
        type = FieldTypes.Color,
        default = "#000000",
    },
}
TerrainTest.editDefs = {
    texture = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_Texture",
    },
    brightnessNoise = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_BrightnessNoise",
    },
    crackNoise = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_CrackNoise",
    },
    color = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_Color",
    },
    cutoff = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_Cutoff",
        parameter = "0:1:0.01",
    },
    luminanceThreshold = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_LuminanceThreshold",
        parameter = "0:1:0.01",
    },
    brightnessScale = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_BrightnessScale",
        parameter = "0.001:0.1:0.001",
    },
    brightnessStrength = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_BrightnessStrength",
        parameter = "0:4:0.01",
    },
    brightnessQuantizationSteps = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_BrightnessQuantizationSteps",
        parameter = "1:64:1",
    },
    brightnessColor = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_BrightnessColor",
    },
}

TerrainTest.defaultLayer = 0
TerrainTest.placementType = PlacementTypes.Single

function TerrainTest:nodeAt(info, index, nodeCount)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Opaque,
        anchor = { x = 0.5, y = 0.5, },
        materialKey = "Terrain",
        color = info.color,
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = info.texture,
            },
            u_NoiseBrightness = {
                type = UniformTypes.Texture,
                value = info.brightnessNoise,
            },
            u_Cutoff = {
                type = UniformTypes.Float1,
                value = info.cutoff,
            },
            u_BrightnessArgs = {
                type = UniformTypes.Float4,
                value = { x = info.brightnessScale, y = info.brightnessStrength, z = info.brightnessQuantizationSteps, w = info.luminanceThreshold, },
            },
            u_BrightnessColor = {
                type = UniformTypes.Float4,
                value = info.brightnessColor,
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = info.texture,
        },
        overrideTextureFilters = {
            u_NoiseBrightness = BuiltinTextureFilters.PixelRepeat,
        },
    })
    return ret
end

return TerrainTest