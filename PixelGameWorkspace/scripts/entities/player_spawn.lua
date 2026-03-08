local PlayerSpawn = Class("PlayerSpawn")
local Helper = require("helper_functions")

PlayerSpawn.group = "SG_FeatureNodes"
PlayerSpawn.name = "SN_PlayerSpawn"
PlayerSpawn.tooltip = "ST_PlayerSpawn"
PlayerSpawn.dataDefs = {
    isGameStart = {
        type = FieldTypes.Boolean,
        default = false,
    },
}
PlayerSpawn.editDefs = {
    isGameStart = {
        tooltip = "ST_PlayerSpawn_IsGameStart",
    },
}

PlayerSpawn.defaultLayer = -10
PlayerSpawn.placementType = PlacementTypes.Single

function PlayerSpawn:canRotate(index)
    return false
end

function PlayerSpawn:canScale(index)
    return false
end

function PlayerSpawn:nodeAt(info, index, nodeCount)
    local ret = {}
    local texPlayer = Helper.localTexture("textures/scene/characters/fox_test.png")
    table.insert(ret, {
        stage = BuiltinStages.Transparent,
        anchor = { x = 0.5, y = 1.0, },
        materialKey = "Standard",
        color = "#ffffff",
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = texPlayer,
            },
        },
        sizer = {
            type = SizerTypes.Texture,
            texture = texPlayer,
        },
    })
    return ret
end

return PlayerSpawn