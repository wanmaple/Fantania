local FrameAnimation = Class("FrameAnimation");

FrameAnimation.name = "SN_FrameAnimation"
FrameAnimation.tooltip = "ST_FrameAnimation"
FrameAnimation.dataGroup = "Animation"
FrameAnimation.dataDefs = {
    atlas = {
        type = FieldTypes.StringArray,
    },
    interval = {
        type = FieldTypes.Float,
        default = 0.1,
    },
    frameDiscription = {
        type = FieldTypes.String,
        default = "1-12",
    },
    loop = {
        type = FieldTypes.Boolean,
        default = true,
    },
    next = {
        type = FieldTypes.GroupReference,
        default = {
            group = "Animation",
            id = 0,
        },
    },
}
FrameAnimation.editDefs = {
    atlas = {
        group = "SG_Data",
        tooltip = "ST_FrameAnimation_Atlas",
        control = "FantaniaLib.AtlasBox",
        validator = "FantaniaLib.AtlasValidator",
    },
    interval = {
        group = "SG_Data",
        tooltip = "ST_FrameAnimation_Interval",
        parameter = "0.01:1.0:0.01",
    },
    frameDiscription = {
        group = "SG_Data",
        tooltip = "ST_FrameAnimation_FrameDescription",
        validator = "FantaniaLib.FrameDescriptionValidator",
    },
    loop = {
        group = "SG_Data",
        tooltip = "ST_FrameAnimation_Loop",
    },
    next = {
        group = "SG_Data",
        tooltip = "ST_FrameAnimation_Next",
    },
}

return FrameAnimation;