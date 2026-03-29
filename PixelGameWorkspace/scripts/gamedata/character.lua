local Character = Class("Character");

Character.name = "SN_Character"
Character.tooltip = "ST_Character"
Character.dataGroup = "Pawn"
Character.dataDefs = {
    animationNames = {
        type = FieldTypes.StringArray,
    },
    animations = {
        type = FieldTypes.GroupReferenceArray,
    },
    startupAnimation = {
        type = FieldTypes.String,
        default = "idle",
    },
    size = {
        type = FieldTypes.Vector2Int,
        default = { x = 64, y = 64, },
    },
    color = {
        type = FieldTypes.Color,
        default = "#ffffff",
    },
    cutoff = {
        type = FieldTypes.Float,
        default = 0.1,
    },
}
Character.editDefs = {
    animationNames = {
        group = "SG_Data",
        tooltip = "ST_Character_AnimationNames",
        defaultMemberValue = "idle",
        validator = "FantaniaLib.DistinctValidator",
    },
    animations = {
        group = "SG_Data",
        tooltip = "ST_Character_Animations",
        defaultMemberValue = {
            group = "Animation",
            id = 0,
        },
    },
    startupAnimation = {
        group = "SG_Data",
        tooltip = "ST_Character_StartupAnimation",
    },
    color = {
        group = "SG_Data",
        tooltip = "ST_Character_Color",
    },
    size = {
        group = "SG_Data",
        tooltip = "ST_Character_Size",
        parameter = "8:4096:8",
    },
    cutoff = {
        group = "SG_Data",
        tooltip = "ST_Character_Cutoff",
        parameter = "0.01:1.0:0.01",
    },
}

return Character
