local ConnectedSprites = Class("ConnectedSprites")
local MathsHelper = require("maths_helper")

ConnectedSprites.group = "SG_Decoration"
ConnectedSprites.name = "SN_ConnectedSprites"
ConnectedSprites.tooltip = "ST_ConnectedSprites"
ConnectedSprites.dataDefs = {
    diffuse = {
        type = FieldTypes.Texture,
    },
    anchor = {
        type = FieldTypes.Vector2,
        default = { x = 0.5, y = 1.0, },
    },
    connSize = {
        type = FieldTypes.Integer,
        default = 4,
    },
    connOffset = {
        type = FieldTypes.Vector2Int,
        default = { x = 0, y = -200, },
    },
    connColor = {
        type = FieldTypes.Color,
        default = "#ffffff",
    },
}
ConnectedSprites.editDefs = {
    diffuse = {
        group = "SG_Appearance",
        tooltip = "ST_ConnectedSprites_Diffuse",
    },
    anchor = {
        group = "SG_Transform",
        tooltip = "ST_ConnectedSprites_Anchor",
    },
    connSize = {
        group = "SG_Appearance",
        tooltip = "ST_ConnectedSprites_ConnSize",
        parameter = "1:100:1",
    },
    connOffset = {
        group = "SG_Transform",
        tooltip = "ST_ConnectedSprites_ConnOffset",
    },
    connColor = {
        group = "SG_Appearance",
        tooltip = "ST_ConnectedSprites_ConnColor",
    },
}

ConnectedSprites.defaultLayer = 0
ConnectedSprites.placementType = PlacementTypes.MultiNodes
ConnectedSprites.nodeOptions = {
    min = 1,
    max = -1,
    defaultOffset = { x = 256, y = 0, },
}

function ConnectedSprites:canRotate(index)
    return false
end

function ConnectedSprites:canScale(index)
    return false
end

function ConnectedSprites:nodeAt(info, index, nodeCount)
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

function ConnectedSprites:foregroundNodes(info, nodes)
    local ret = {}
    for i = 1, #nodes - 1, 1 do
        local nd1 = nodes[i]
        local nd2 = nodes[i + 1]
        local width = MathsHelper.distanceOfVec2(nd1.position, nd2.position)
        local height = info.connSize
        table.insert(ret, {
            stage = BuiltinStages.Transparent,
            anchor = { x = 0.0, y = 0.5, },
            position = MathsHelper.addVec2(nd1.position, info.connOffset),
            rotation = MathsHelper.radianOfVec2(MathsHelper.subVec2(nd2.position, nd1.position)),
            materialKey = "PureColor",
            color = info.connColor,
            uniforms = {
            },
            sizer = {
                type = SizerTypes.Fixed,
                size = { x = width, y = height, }
            },
        })
    end
    return ret
end

return ConnectedSprites