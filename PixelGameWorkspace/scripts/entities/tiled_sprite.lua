local TiledSprite = Class("TiledSprite")
local TilingHelper = require("tiling_helper")

TiledSprite.group = "SG_Decoration"
TiledSprite.name = "SN_TiledSprite"
TiledSprite.tooltip = "ST_TiledSprite"
TiledSprite.dataDefs = {
    diffuse = {
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
    tileSize = {
        type = FieldTypes.Vector2Int,
        default = { x = 64, y = 64, },
    }
}
TiledSprite.editDefs = {
    diffuse = {
        group = "SG_Appearance",
        tooltip = "ST_TiledSprite_Diffuse",
    },
    color = {
        group = "SG_Appearance",
        tooltip = "ST_TiledSprite_Color",
    },
    cutoff = {
        group = "SG_Appearance",
        tooltip = "ST_TiledSprite_Cutoff",
        parameter = "0.0:1.0:0.01",
    },
    tileSize = {
        group = "SG_Appearance",
        tooltip = "ST_TiledSprite_TileSize",
    },
}

TiledSprite.defaultLayer = -10
TiledSprite.placementType = PlacementTypes.Tiled
TiledSprite.tileSize = { x = 64, y = 64, }

function TiledSprite:tileAt(placement, size, locType, hash)
    local uvOffset = TilingHelper.getUVOffset(locType, hash, placement.tileSize)
    return {
        stage = BuiltinStages.Opaque,
        material = "StandardCutoff",
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = placement.diffuse,
            },
            u_Cutoff = {
                type = UniformTypes.Float1,
                value = placement.cutoff,
            },
        },
        uvOffset = uvOffset,
        uvSize = TilingHelper.getTileUVSize(placement.tileSize),
        color = placement.color,
    }
end

return TiledSprite