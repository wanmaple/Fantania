local ConnectedSprites = Class("ConnectedSprites")

ConnectedSprites.group = "SG_Decoration"
ConnectedSprites.name = "SN_ConnectedSprites"
ConnectedSprites.tooltip = "ST_ConnectedSprites"
ConnectedSprites.dataDefs = {
    diffuse = {
        type = FieldTypes.Texture,
    },
    -- normal = {
    --     type = FieldTypes.Texture,
    -- },
    -- lit = {
    --     type = FieldTypes.Boolean,
    -- },
    -- color = {
    --     type = FieldTypes.Color,
    --     -- default = { r = 1.0, g = 0.0, b = 0.0, a = 1.0, },
    --     default = "#ffffff",
    -- },
    anchor = {
        type = FieldTypes.Vector2,
        default = { x = 0.5, y = 1.0, },
    },
}
ConnectedSprites.editDefs = {
    diffuse = {
        group = "SG_Appearance",
        tooltip = "ST_ConnectedSprites_Diffuse",
    },
    -- normal = {
    --     group = "SG_Appearance",
    --     tooltip = "ST_ConnectedSprites_Normal",
    -- },
    -- lit = {
    --     group = "SG_Appearance",
    --     tooltip = "ST_ConnectedSprites_Lit",
    -- },
    -- color = {
    --     group = "SG_Appearance",
    --     tooltip = "ST_ConnectedSprites_Color",
    -- },
    anchor = {
        group = "SG_Transform",
        tooltip = "ST_ConnectedSprites_Anchor",
    },
}

ConnectedSprites.defaultLayer = 0
ConnectedSprites.placementType = PlacementTypes.MultiNodes
ConnectedSprites.nodeOptions = {
    min = 1,
    max = -1,
    defaultOffset = { x = 64, y = 0, },
}

function ConnectedSprites:nodeAt(info, index)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Transparent,
        anchor = info.anchor,
        materialKey = "Standard",
        color = "#ffffff",
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

return ConnectedSprites