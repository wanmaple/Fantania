local StaticImage = Class("StaticImage")

StaticImage.group = "SG_Decoration"
StaticImage.name = "SN_StaticImage"
StaticImage.tooltip = "ST_StaticImage"
StaticImage.dataDefs = {
    diffuse = {
        type = FieldTypes.Texture,
    },
    -- normal = {
    --     type = FieldTypes.Texture,
    -- },
    lit = {
        type = FieldTypes.Boolean,
    },
    number = {
        type = FieldTypes.Vector2,
        default = { x = 1.0, y = 0.0, },
    },
    veci = {
        type = FieldTypes.Vector2Int,
        default = { x = 3, y = -2, },
    },
    color = {
        type = FieldTypes.Color,
        -- default = { r = 1.0, g = 0.0, b = 0.0, a = 1.0, },
        default = "#FF0000",
    },
}
StaticImage.editDefs = {
    diffuse = {
        group = "SG_Appearance",
        tooltip = "ST_StaticImage_Diffuse",
    },
    -- normal = {
    --     group = "SG_Appearance",
    --     tooltip = "ST_StaticImage_Normal",
    -- },
    lit = {
        group = "SG_Appearance",
        tooltip = "ST_StaticImage_Lit",
    },
    number = {
        group = "SG_Appearance",
        tooltip = "ST_StaticImage_Num",
        parameter = "-2.0:2.0:0.2",
    },
    veci = {
        group = "SG_Appearance",
        tooltip = "ST_StaticImage_Veci",
        parameter = "-100:100",
    },
    color = {
        group = "SG_Appearance",
        tooltip = "ST_StaticImage_Color",
        control = EditControls.ColorPicker,
    },
}

StaticImage.defaultAnchor = { x = 0.5, y = 1.0, }
StaticImage.defaultLayer = 0
StaticImage.nodeOptions = {
    min = 0,
    max = 0,
    defaultOffset = { x = 100, y = 0, },
}

local dump = require "dump"

function StaticImage:renderInfo(info, nodes)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Transparent,
        materialKey = "Standard",
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = info.diffuse,
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = info.diffuse,
        },
    })
    return ret
end

return StaticImage