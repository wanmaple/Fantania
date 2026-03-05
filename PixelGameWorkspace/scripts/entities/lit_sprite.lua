local LitSprite = Class("LitSprite")

LitSprite.group = "SG_Decoration"
LitSprite.name = "SN_LitSprite"
LitSprite.tooltip = "ST_LitSprite"
LitSprite.dataDefs = {
    albedo = {
        type = FieldTypes.Texture,
    },
    normal = {
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
    anchor = {
        type = FieldTypes.Vector2,
        default = { x = 0.5, y = 1.0, },
    },
}
LitSprite.editDefs = {
    albedo = {
        group = "SG_Appearance",
        tooltip = "ST_LitSprite_Albedo",
    },
    normal = {
        group = "SG_Appearance",
        tooltip = "ST_LitSprite_Normal",
    },
    color = {
        group = "SG_Appearance",
        tooltip = "ST_LitSprite_Color",
    },
    cutoff = {
        group = "SG_Appearance",
        tooltip = "ST_LitSprite_Cutoff",
        parameter = "0:1:0.01",
    },
    anchor = {
        group = "SG_Transform",
        tooltip = "ST_LitSprite_Anchor",
    },
}

LitSprite.defaultLayer = 0
LitSprite.placementType = PlacementTypes.Single

function LitSprite:nodeAt(info, index, nodeCount)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Opaque,
        anchor = info.anchor,
        materialKey = "StandardLit",
        color = info.color,
        uniforms = {
            u_Albedo = {
                type = UniformTypes.Texture,
                value = info.albedo,
            },
            u_Normal = {
                type = UniformTypes.Texture,
                value = info.normal,
            },
            u_Cutoff = {
                type = UniformTypes.Float1,
                value = info.cutoff,
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = info.albedo,
        },
    })
    table.insert(ret, {
        stage = BuiltinStages.LightOccluderSDF,
        anchor = info.anchor,
        materialKey = "LightOccluderMask",
        color = info.color,
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = info.albedo,
            },
            u_Cutoff = {
                type = UniformTypes.Float1,
                value = info.cutoff,
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = info.albedo,
        },
    })
    return ret
end

return LitSprite