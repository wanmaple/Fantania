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
        default = "#ffffff",
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
        default = 0.003,
    },
    brightnessStrength = {
        type = FieldTypes.Float,
        default = 0.22,
    },
    brightnessQuantizationSteps = {
        type = FieldTypes.Integer,
        default = 8,
    },
    brightnessColor = {
        type = FieldTypes.Color,
        default = "#ffffff",
    },
    crackScale = {
        type = FieldTypes.Float,
        default = 0.003,
    },
    crackThresholdMin = {
        type = FieldTypes.Float,
        default = 0.82,
    },
    crackThresholdMax = {
        type = FieldTypes.Float,
        default = 0.90,
    },
    crackSharpness = {
        type = FieldTypes.Float,
        default = 2.4,
    },
    crackStrength = {
        type = FieldTypes.Float,
        default = 0.10,
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
    crackScale = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_CrackScale",
        parameter = "0.001:10:0.001",
    },
    crackThresholdMin = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_CrackThresholdMin",
        parameter = "0:1:0.01",
    },
    crackThresholdMax = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_CrackThresholdMax",
        parameter = "0:1:0.01",
    },
    crackSharpness = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_CrackSharpness",
        parameter = "1:10:0.1",
    },
    crackStrength = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_CrackStrength",
        parameter = "0:1:0.01",
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
            u_NoiseCrack = {
                type = UniformTypes.Texture,
                value = info.crackNoise,
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
                value = { info.brightnessColor.x, info.brightnessColor.y, info.brightnessColor.z, info.crackScale, },
            },
            u_CrackArgs = {
                type = UniformTypes.Float4,
                value = { x = info.crackThresholdMin, y = info.crackThresholdMax, z = info.crackSharpness, w = info.crackStrength, },
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = info.texture,
        },
        overrideTextureFilters = {
            u_NoiseBrightness = BuiltinTextureFilters.PixelRepeat,
            u_NoiseCrack = BuiltinTextureFilters.PixelRepeat,
        },
    })
    return ret
end

return TerrainTest