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

return HelperFunctions