local GlobalColors = Class("GlobalColors");

GlobalColors.name = "SN_GlobalColors"
GlobalColors.tooltip = "ST_GlobalColors"
GlobalColors.dataGroup = "Globals"
GlobalColors.dataDefs = {
    key = {
        type = FieldTypes.String,
    },
    value = {
        type = FieldTypes.Color,
        default = "#ffffff"
    },
}
GlobalColors.editDefs = {
    key = {
        group = "SG_Data",
        tooltip = "ST_GlobalColors_Key",
    },
    value = {
        group = "SG_Data",
        tooltip = "ST_GlobalColors_Value",
    },
}

return GlobalColors;