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

local function Hash(x, y, seed)
    local s = seed or 0
    local h = x + 0x9e3779b9
    h = bit32.bxor(h, bit32.lshift(h, 6))
    h = bit32.bxor(h, bit32.rshift(h, 2))
    h = bit32.bxor(h, bit32.lshift(h, 7))
    h = bit32.bxor(h, bit32.rshift(h, 3))
    h = bit32.bxor(h, bit32.lshift(h, 13))
    h = bit32.bxor(h, bit32.rshift(h, 11))
    return h
end

function TiledSprite:tileAt(placement, size, x, y, hash)
    local uvOffset
    if x == 0 and y == 0 then
        uvOffset = { x = 0.0, y = 0.0, }
    elseif x == size.x - 1 and y == 0 then
        uvOffset = { x = 1.0 - 1.0 / placement.split.x, y = 0.0, }
    elseif x == 0 and y == size.y - 1 then
        uvOffset = { x = 0.0, y = 1.0 - 1.0 / placement.split.y, }
    elseif x == size.x - 1 and y == size.y - 1 then
        uvOffset = { x = 1.0 - 1.0 / placement.split.x, y = 1.0 - 1.0 / placement.split.y, }
    elseif x == 0 then
        uvOffset = { x = 0.0, y = (hash % (placement.split.y - 2) + 1) / placement.split.y, }
    elseif x == size.x - 1 then
        uvOffset = { x = 1.0 - 1.0 / placement.split.x, y = (hash % (placement.split.y - 2) + 1) / placement.split.y, }
    elseif y == 0 then
        uvOffset = { x = (hash % (placement.split.x - 2) + 1) / placement.split.x, y = 0.0, }
    elseif y == size.y - 1 then
        uvOffset = { x = (hash % (placement.split.x - 2) + 1) / placement.split.x, y = 1.0 - 1.0 / placement.split.y, }
    else
        uvOffset = { x = (hash % (placement.split.x - 2) + 1) / placement.split.x, y = (hash % (placement.split.y - 2) + 1) / placement.split.y, }
    end
    return {
        stage = BuiltinStages.Opaque,
        material = "Standard",
        uniforms = {
            u_Texture = {
                type = UniformTypes.Texture,
                value = placement.diffuse,
            },
        },
        uvOffset = uvOffset,
        uvSize = { x = 1.0 / placement.split.x, y = 1.0 / placement.split.y, },
    }
end

return TiledSprite