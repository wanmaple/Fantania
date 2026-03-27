local GlobalParameter = Class("GlobalParameter");

GlobalParameter.name = "SN_GlobalParameter"
GlobalParameter.tooltip = "ST_GlobalParameter"
GlobalParameter.dataGroup = "Globals"
GlobalParameter.dataDefs = {
    key = {
        type = FieldTypes.String,
    },
    value = {
        type = FieldTypes.Float,
        default = 0.0,
    },
}
GlobalParameter.editDefs = {
    key = {
        group = "SG_Data",
        tooltip = "ST_GlobalParameter_Key",
    },
    value = {
        group = "SG_Data",
        tooltip = "ST_GlobalParameter_Value",
        parameter = "-1000000:1000000:0.01",
    },
}

return GlobalParameter;