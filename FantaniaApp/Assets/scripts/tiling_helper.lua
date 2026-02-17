local TilingHelper = {}

local function coordAt(num, tileSize)
    return (num + 0.5 / tileSize) / 8.0
end

function TilingHelper.getUVOffset(locType, hash, tileSize)
    local x, y
    if locType == TileLocationTypes.Center then
        local rd = hash % 9
        x = 2 + rd % 3
        y = 2 + math.floor(rd / 3)
    elseif locType == TileLocationTypes.CornerOuterTopLeft then
        local rd = hash % 4
        x = rd % 2
        y = math.floor(rd / 2)
    elseif locType == TileLocationTypes.CornerOuterTopRight then
        local rd = hash % 4
        x = 6 + rd % 2
        y = math.floor(rd / 2)
    elseif locType == TileLocationTypes.CornerOuterBottomLeft then
        local rd = hash % 4
        x = rd % 2
        y = 6 + math.floor(rd / 2)
    elseif locType == TileLocationTypes.CornerOuterBottomRight then
        local rd = hash % 4
        x = 6 + rd % 2
        y = 6 + math.floor(rd / 2)
    elseif locType == TileLocationTypes.CornerInnerTopLeft then
        x = 4
        y = 7
    elseif locType == TileLocationTypes.CornerInnerTopRight then
        x = 5
        y = 7
    elseif locType == TileLocationTypes.CornerInnerBottomLeft then
        x = 3
        y = 7
    elseif locType == TileLocationTypes.CornerInnerBottomRight then
        x = 2
        y = 7
    elseif locType == TileLocationTypes.EdgeTop then
        local rd = hash % 4
        x = 2 + rd
        y = 1
    elseif locType == TileLocationTypes.EdgeBottom then
        local rd = hash % 4
        x = 2 + rd
        y = 6
    elseif locType == TileLocationTypes.EdgeLeft then
        local rd = hash % 4
        x = 1
        y = 2 + rd
    elseif locType == TileLocationTypes.EdgeRight then
        local rd = hash % 4
        x = 6
        y = 2 + rd
    elseif locType == TileLocationTypes.Single then
        x = 0
        y = 2
    elseif locType == TileLocationTypes.PillarHorizontalLeft then
        x = 2
        y = 5
    elseif locType == TileLocationTypes.PillarHorizontalRight then
        x = 4
        y = 5
    elseif locType == TileLocationTypes.PillarHorizontalCenter then
        x = 3
        y = 5
    elseif locType == TileLocationTypes.PillarVerticalTop then
        x = 5
        y = 2
    elseif locType == TileLocationTypes.PillarVerticalBottom then
        x = 5
        y = 4
    elseif locType == TileLocationTypes.PillarVerticalCenter then
        x = 5
        y = 3
    elseif locType == TileLocationTypes.InnerDiagonalSlash then
        x = 0
        y = 4
    elseif locType == TileLocationTypes.InnerDiagonalBackslash then
        x = 0
        y = 3
    elseif locType == TileLocationTypes.InnerX then
        x = 0
        y = 5
    elseif locType == TileLocationTypes.InnerMissingTopLeft then
        x = 2
        y = 0
    elseif locType == TileLocationTypes.InnerMissingTopRight then
        x = 3
        y = 0
    elseif locType == TileLocationTypes.InnerMissingBottomLeft then
        x = 5
        y = 0
    elseif locType == TileLocationTypes.InnerMissingBottomRight then
        x = 4
        y = 0
    else
        local rd = hash % 9
        x = 2 + rd % 3
        y = 2 + math.floor(rd / 3)
    end
    return { x = coordAt(x, tileSize.x), y = coordAt(y, tileSize.y), }
end

function TilingHelper.getTileUVSize(tileSize)
    return { x = 1.0 / 8.0 - 0.5 / (8.0 * tileSize.x), y = 1.0 / 8.0 - 0.5 / (8.0 * tileSize.y), }
end

return TilingHelper