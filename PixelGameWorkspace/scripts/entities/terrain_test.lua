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
    grainNoise = {
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
    brightnessScale = {
        type = FieldTypes.Float,
        default = 0.003,
    },
    brightnessStrength = {
        type = FieldTypes.Float,
        default = 0.22,
    },
    grainScale = {
        type = FieldTypes.Float,
        default = 0.02,
    },
    grainStrength = {
        type = FieldTypes.Float,
        default = 0.06,
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
    grainNoise = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_GrainNoise",
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
    brightnessScale = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_BrightnessScale",
        parameter = "0.001:0.1:0.001",
    },
    brightnessStrength = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_BrightnessStrength",
        parameter = "0:1:0.01",
    },
    grainScale = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_GrainScale",
        parameter = "0.001:0.1:0.001",
    },
    grainStrength = {
        group = "SG_Appearance",
        tooltip = "ST_TerrainTest_GrainStrength",
        parameter = "0:1:0.01",
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
            u_NoiseGrain = {
                type = UniformTypes.Texture,
                value = info.grainNoise,
            },
            u_Cutoff = {
                type = UniformTypes.Float1,
                value = info.cutoff,
            },
            u_BrightnessGrain = {
                type = UniformTypes.Float4,
                value = { x = info.brightnessScale, y = info.grainScale, z = info.brightnessStrength, w = info.grainStrength, },
            },
            u_Crack = {
                type = UniformTypes.Float4,
                value = { x = info.crackThresholdMin, y = info.crackThresholdMax, z = info.crackSharpness, w = info.crackStrength, },
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = info.texture,
        },
    })
    return ret
end

return TerrainTest