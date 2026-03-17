local HelperFunctions = {}

function HelperFunctions.localTexture(path)
    return {
        texType = TextureTypes.Image,
        texParam = {
            path = path,
        },
    }
end

function HelperFunctions.atlasTexture(atlas, key)
    return {
        texType = TextureTypes.Atlas,
        texParam = {
            atlas = atlas,
            key = key,
        },
    }
end

function HelperFunctions.arrayHasElement(array, element)
    for _, v in ipairs(array) do
        if v == element then
            return true
        end
    end
    return false
end

function HelperFunctions.count(table)
    local count = 0
    for _, _ in pairs(table) do
        count = count + 1
    end
    return count
end

function HelperFunctions.first(array, predicate)
    for _, v in ipairs(array) do
        if predicate(v) then
            return v
        end
    end
    return nil
end

function HelperFunctions.select(array, func)
    local ret = {}
    for _, v in ipairs(array) do
        table.insert(ret, func(v))
    end
    return ret
end

function HelperFunctions.mergeArray(...)
    local args = {...}
    if (#args == 0) then
        return {}
    end
    local ret = args[1]
    for i = 2, #args do
        local array = args[i]
        for _, element in ipairs(array) do
            table.insert(ret, element)
        end
    end
    return ret
end

function HelperFunctions.combinePath(...)
    local args = {...}
    if #args == 0 then
        return ""
    end
    local path = args[1]
    if path[#path] ~= "/" then
        path = path .. "/"
    end
    for i = 2, #args - 1 do
        local arg = args[i]
        if arg[#arg] ~= "/" then
            arg = arg .. "/"
        end
        path = path .. arg
    end
    path = path .. args[#args]
    return path
end

return HelperFunctions