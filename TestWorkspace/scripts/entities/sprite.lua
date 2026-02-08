local Sprite = Class("Sprite")

Sprite.group = "SG_Decoration"
Sprite.name = "SN_Sprite"
Sprite.tooltip = "ST_Sprite"
Sprite.dataDefs = {
    diffuse = {
        type = FieldTypes.Texture,
    },
    normal = {
        type = FieldTypes.Texture,
    },
    lit = {
        type = FieldTypes.Boolean,
    },
    color = {
        type = FieldTypes.Color,
        -- default = { r = 1.0, g = 0.0, b = 0.0, a = 1.0, },
        default = "#ffffff",
    },
    anchor = {
        type = FieldTypes.Vector2,
        default = { x = 0.5, y = 1.0, },
    },
}
Sprite.editDefs = {
    diffuse = {
        group = "SG_Appearance",
        tooltip = "ST_Sprite_Diffuse",
    },
    normal = {
        group = "SG_Appearance",
        tooltip = "ST_Sprite_Normal",
    },
    lit = {
        group = "SG_Appearance",
        tooltip = "ST_Sprite_Lit",
    },
    color = {
        group = "SG_Appearance",
        tooltip = "ST_Sprite_Color",
    },
    anchor = {
        group = "SG_Transform",
        tooltip = "ST_Sprite_Anchor",
    },
}

Sprite.defaultLayer = 0
Sprite.placementType = PlacementTypes.Single

function Sprite:nodeAt(info, index, nodeCount)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Transparent,
        anchor = info.anchor,
        materialKey = "Standard",
        color = info.color,
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

return Sprite