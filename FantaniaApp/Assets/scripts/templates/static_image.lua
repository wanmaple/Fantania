local StaticImage = Class("StaticImage")

function StaticImage:group()
    return "SG_Decoration"
end

function StaticImage:name()
    return "SN_StaticImage"
end

function StaticImage:tooltip()
    return "ST_StaticImage"
end

function StaticImage:data()
    return {
        diffuse = {
            type = FieldTypes.Texture,
            group = "SG_Appearance",
        },
        normal = {
            type = FieldTypes.Texture,
            group = "SG_Appearance",
        },
        lit = {
            type = FieldTypes.Boolean,
            group = "SG_Appearance",
        },
    }
end

function StaticImage:nodeOptions()
    return {
        min = 0,
        max = 0,
    }
end

function StaticImage:renderNodes(entity)
    entity:addRenderNode({
        stage = RenderStages.Transparency,
        -- material = 
    })
end

return StaticImage