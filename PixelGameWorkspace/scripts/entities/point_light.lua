local PointLight = Class("PointLight")
local Helper = require("helper_functions")

PointLight.group = "SG_Lights"
PointLight.name = "SN_PointLight"
PointLight.tooltip = "ST_PointLight"
PointLight.dataDefs = {
    icon = {
        type = FieldTypes.Texture,
    },
    texture = {
        type = FieldTypes.Texture,
    },
    color = {
        type = FieldTypes.Color,
        default = "#ffffff",
    },
    radius = {
        type = FieldTypes.Integer,
        default = 128,
    },
}
PointLight.editDefs = {
    icon = {
        group = "SG_Appearance",
        tooltip = "ST_PointLight_Icon",
    },
    texture = {
        group = "SG_Appearance",
        tooltip = "ST_PointLight_Texture",
    },
    color = {
        group = "SG_Appearance",
        tooltip = "ST_PointLight_Color",
    },
    radius = {
        group = "SG_Appearance",
        tooltip = "ST_PointLight_Radius",
        parameter = "8:10000:8",
    },
}

PointLight.defaultLayer = 0
PointLight.placementType = PlacementTypes.LightSource

function PointLight:canRotate(index)
    return false
end

function PointLight:nodeAt(info, index, nodeCount)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Transparent,
        anchor = { x = 0.5, y = 0.5, },
        materialKey = "Standard",
        color = "#ffffff",
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = info.icon,
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = info.icon,
        },
    })
    table.insert(ret, {
        stage = BuiltinStages.TiledLightCulling,
        anchor = { x = 0.5, y = 0.5, },
        materialKey = "Standard",
        color = info.color,
        renderableType = "FantaniaLib.LightSource",
        customArgs = {
            lightTexture = {
                type = FieldTypes.Texture,
                value = info.texture,
            },
            radius = {
                type = FieldTypes.Integer,
                value = info.radius,
            },
            color = {
                type = FieldTypes.Color,
                value = info.color,
            },
        },
    })
    return ret
end

return PointLight