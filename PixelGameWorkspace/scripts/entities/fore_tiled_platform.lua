local ForeTiledPlatform = Class("ForeTiledPlatform")
local TilingHelper = require("tiling_helper")

ForeTiledPlatform.group = "SG_Platforms"
ForeTiledPlatform.name = "SN_ForeTiledPlatform"
ForeTiledPlatform.tooltip = "ST_ForeTiledPlatform"
ForeTiledPlatform.dataDefs = {
    texture = {
        type = FieldTypes.Texture,
    },
    brightnessNoise = {
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
        default = "#ffffff",
    },
}
ForeTiledPlatform.editDefs = {
    texture = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_Texture",
    },
    brightnessNoise = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_BrightnessNoise",
    },
    color = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_Color",
    },
    cutoff = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_Cutoff",
        parameter = "0:1:0.01",
    },
    luminanceThreshold = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_LuminanceThreshold",
        parameter = "0:1:0.01",
    },
    brightnessScale = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_BrightnessScale",
        parameter = "0.001:0.1:0.001",
    },
    brightnessStrength = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_BrightnessStrength",
        parameter = "0:4:0.01",
    },
    brightnessQuantizationSteps = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_BrightnessQuantizationSteps",
        parameter = "1:64:1",
    },
    brightnessColor = {
        group = "SG_Appearance",
        tooltip = "ST_ForeTiledPlatform_BrightnessColor",
    },
}

ForeTiledPlatform.defaultLayer = -10
ForeTiledPlatform.placementType = PlacementTypes.Tiled
ForeTiledPlatform.tileSize = { x = 128, y = 128, }

function ForeTiledPlatform:tileAt(placement, size, locType, hash)
    local uvOffset = TilingHelper.getUVOffset(locType, hash)
    return {
        stage = BuiltinStages.Opaque,
        materialKey = "ForeTerrain",
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = placement.texture,
            },
            u_NoiseBrightness = {
                type = UniformTypes.Texture,
                value = placement.brightnessNoise,
            },
            u_Cutoff = {
                type = UniformTypes.Float1,
                value = placement.cutoff,
            },
            u_BrightnessArgs = {
                type = UniformTypes.Float4,
                value = { x = placement.brightnessScale, y = placement.brightnessStrength, z = placement.brightnessQuantizationSteps, w = placement.luminanceThreshold, },
            },
            u_BrightnessColor = {
                type = UniformTypes.Float4,
                value = placement.brightnessColor,
            },
        },
        uvOffset = uvOffset,
        uvSize = { x = 0.125, y = 0.125, },
        color = placement.color,
        overrideTextureFilters = {
            u_NoiseBrightness = BuiltinTextureFilters.PixelRepeat,
        },
    }
end

return ForeTiledPlatform