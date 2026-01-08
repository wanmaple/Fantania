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

function StaticImage:dataDefs()
    return {
        -- diffuse = {
        --     type = FieldTypes.Texture,
        --     default = FantaniaTextures.White4x4,
        -- },
        -- normal = {
        --     type = FieldTypes.Texture,
        -- },
        lit = {
            type = FieldTypes.Boolean,
            default = false,
        },
        number = {
            type = FieldTypes.Vector2,
            default = { x = 1.0, y = 0.0, },
        },
        color = {
            type = FieldTypes.Color,
            default = { r = 1.0, g = 0.0, b = 0.0, a = 1.0, },
        },
    }
end

function StaticImage:editDefs()
    return {
        -- diffuse = {
        --     group = "SG_Appearance",
        --     tooltip = "ST_StaticImage_Diffuse",
        -- },
        -- normal = {
        --     group = "SG_Appearance",
        --     tooltip = "ST_StaticImage_Normal",
        -- },
        lit = {
            group = "SG_Appearance",
            tooltip = "ST_StaticImage_Lit",
        },
        number = {
            group = "SG_Appearance",
            tooltip = "ST_StaticImage_Num",
            parameter = "-2.0:2.0:0.2",
        },
        color = {
            group = "SG_Appearance",
            tooltip = "ST_StaticImage_Color",
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