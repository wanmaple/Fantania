local TiledSprite = Class("TiledSprite")

TiledSprite.group = "SG_Decoration"
TiledSprite.name = "SN_TiledSprite"
TiledSprite.tooltip = "ST_TiledSprite"
TiledSprite.dataDefs = {
    diffuse = {
        type = FieldTypes.Texture,
    },
    split = {
        type = FieldTypes.Vector2Int,
        default = { x = 1, y = 1, },
    },
}
TiledSprite.editDefs = {
    diffuse = {
        group = "SG_Appearance",
        tooltip = "ST_TiledSprite_Diffuse",
    },
    split = {
        group = "SG_Appearance",
        tooltip = "ST_TiledSprite_Split",
        validator = "FantaniaLib.TileSplitValidator",
    },
}

TiledSprite.defaultLayer = -10
TiledSprite.placementType = PlacementTypes.Tiled
TiledSprite.tileSize = { x = 64, y = 64, }

function TiledSprite:tileAt(placement, size, x, y)
    return {
        stage = BuiltinStages.Opaque,
        material = "Standard",
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = placement.diffuse,
            },
        },
        uvOffset = { x = x / placement.split.x, y = y / placement.split.y, },
        uvSize = { x = 1.0 / placement.split.x, y = 1.0 / placement.split.y, },
    }
end

return TiledSprite