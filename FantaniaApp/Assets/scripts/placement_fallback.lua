local PlacementFallback = Class("PlacementFallback")

PlacementFallback.group = ""
PlacementFallback.name = ""
PlacementFallback.tooltip = ""
PlacementFallback.dataDefs = {
}
PlacementFallback.editDefs = {
}

PlacementFallback.multiNodes = true
PlacementFallback.nodeOptions = {
    min = 0,
    max = -1,
    defaultOffset = { x = 100, y = 0, },
}
PlacementFallback.canTranslate = true
PlacementFallback.canRotate = true
PlacementFallback.canScale = true

function PlacementFallback:nodeAt(info, index)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Opaque,
        materialKey = "#FantaniaFallback",    -- something not exist.
        sizer = {
            type = SizerTypes.Fixed,
            size = { x = 100, y = 100, },
        },
    })
    return ret
end

function PlacementFallback:renderInfo(info, nodes)
    local ret = {}
    table.insert(ret, {
        stage = BuiltinStages.Opaque,
        materialKey = "#FantaniaFallback",    -- something not exist.
        sizer = {
            type = SizerTypes.Fixed,
            size = { x = 100, y = 100, },
        },
    })
    return ret
end

return PlacementFallback