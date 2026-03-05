local Sprite = Class("Sprite")

Sprite.group = "SG_Decoration"
Sprite.name = "SN_Sprite"
Sprite.tooltip = "ST_Sprite"
Sprite.dataDefs = {
    texture = {
        type = FieldTypes.Texture,
    },
    color = {
        type = FieldTypes.Color,
        default = "#ffffff",
    },
    anchor = {
        type = FieldTypes.Vector2,
        default = { x = 0.5, y = 1.0, },
    },
}
Sprite.editDefs = {
    texture = {
        group = "SG_Appearance",
        tooltip = "ST_Sprite_Texture",
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
    if not info.lit then
        table.insert(ret, {
            stage = BuiltinStages.Transparent,
            anchor = info.anchor,
            materialKey = "Standard",
            color = info.color,
            uniforms = {
                u_Texture = {
                    type = UniformTypes.Texture,
                    value = info.texture,
                },
            },
            sizer = {
                type = SizerTypes.Texture,
                texture = info.texture,
            },
        })
    end
    return ret
end

return Sprite