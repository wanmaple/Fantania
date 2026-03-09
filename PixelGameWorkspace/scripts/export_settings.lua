local ExportSettings = Class("ExportSettings")
local PipelineHook = require("pipeline_hook")

ExportSettings.group = "SG_Export"
ExportSettings.name = "SN_ExportSettings"
ExportSettings.tooltip = "ST_ExportSettings"
ExportSettings.dataDefs = {
    defaultLevelName = {
        type = FieldTypes.String,
        default = "",
    },
    globalUniforms = {
        type = FieldTypes.StringArray,
        default = {
            "u_LightLayerDepth",
            "u_ShadowArguments",
        },
    },
    exceptLevels = {
        type = FieldTypes.StringArray,
        default = {},
    },
}
ExportSettings.editDefs = {
    defaultLevelName = {
        group = "",
        tooltip = "ST_ExportSettings_DefaultLevelName",
    },
    globalUniforms = {
        group = "",
        tooltip = "ST_ExportSettings_GlobalUniforms",
    },
    exceptLevels = {
        group = "",
        tooltip = "ST_ExportSettings_ExceptLevels",
    },
}

function ExportSettings.globalUniforms()
    local uniforms = {}
end

return ExportSettings