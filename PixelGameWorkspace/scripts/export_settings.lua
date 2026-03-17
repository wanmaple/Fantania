local ExportSettings = Class("ExportSettings")
local PipelineHook = require("pipeline_hook")
local Helper = require("helper_functions")
local ExportHelper = require("export_helper")

ExportSettings.group = "SG_Export"
ExportSettings.name = "SN_ExportSettings"
ExportSettings.tooltip = "ST_ExportSettings"
ExportSettings.dataDefs = {
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
    globalUniforms = {
        group = "",
        tooltip = "ST_ExportSettings_GlobalUniforms",
    },
    exceptLevels = {
        group = "",
        tooltip = "ST_ExportSettings_ExceptLevels",
    },
}

local function getGlobalUniformVariants(uniforms)
    local ret = {}
    for _, uniform in ipairs(PipelineHook.uniforms) do
        if Helper.arrayHasElement(uniforms, uniform.name) then
            local value = uniform.value
            if uniform.type == PipelineHookUniformTypes.Float1 then
                value = { x = value.x, y = 0.0, z = 0.0, w = 0.0, }
                table.insert(ret, {
                    type = FieldTypes.Color,
                    value = value,
                })
            elseif uniform.type == PipelineHookUniformTypes.Float2 then
                value = { x = value.x, y = value.y, z = 0.0, w = 0.0, }
                table.insert(ret, {
                    type = FieldTypes.Color,
                    value = value,
                })
            elseif uniform.type == PipelineHookUniformTypes.Float3 then
                value = { x = value.x, y = value.y, z = value.z, w = 0.0, }
                table.insert(ret, {
                    type = FieldTypes.Color,
                    value = value,
                })
            elseif uniform.type == PipelineHookUniformTypes.Float4 then
                table.insert(ret, {
                    type = FieldTypes.Color,
                    value = value,
                })
            end
        end
    end
    return ret
end

local function getPlacementVariants(placements, remappedPlacements)
    local ret = {}
    local structures = {}
    for name, placement in pairs(placements) do
        remappedPlacements[name] = remappedPlacements[name] or {}
        local newId = 0
        local fields = {}
        for id, props in pairs(placement) do
            remappedPlacements[name][id] = newId
            newId = newId + 1
            local hasFields = #fields > 0
            for _, prop in ipairs(props) do
                if prop.name ~= "id" then
                    table.insert(ret, prop.value)
                    if not hasFields then
                        table.insert(fields, {
                            name = prop.name,
                            type = prop.value.type,
                        })
                    end
                end
            end
        end
        table.insert(structures, {
            name = name,
            fields = fields,
        })
    end
    return ret, structures
end

function ExportSettings:gamedataPath(data)
    return Helper.combinePath(data.ProjectFolder, "assets", "game.ftbin")
end

function ExportSettings:levelFolder(data)
    return Helper.combinePath(data.ProjectFolder, "assets", "levels")
end

function ExportSettings:assetFolder(data)
    return Helper.combinePath(data.ProjectFolder, "assets")
end

function ExportSettings:replacedEmbeddedAssets(data)
    return {
        ["avares://Fantania/Assets/textures/white4x4.png"] = "textures/common/white4x4.png",
        ["avares://Fantania/Assets/textures/black4x4.png"] = "textures/common/black4x4.png",
        ["avares://Fantania/Assets/textures/flat4x4.png"] = "textures/common/flat4x4.png",
    }
end

local PlacementStructures = nil
local PlacementRemap = nil

function ExportSettings:gamedataVariants(data)
    local projFolder = data.ProjectFolder
    local srcCodeFolder = data.SourceCodeFolder
    local globalUniforms = data.globalUniforms
    local exceptLevels = data.exceptLevels
    local allLvs = Workspace.AllLevelNames
    if #allLvs == 0 then
        Workspace.ThrowException("No levels found in the workspace.")
    end
    local ret = {}
    local uniforms = getGlobalUniformVariants(globalUniforms)
    Helper.mergeArray(ret, uniforms)
    -- write levels but put the default level first, and except levels will be ignored
    local sorted = {}
    for _, lv in ipairs(allLvs) do
        if not Helper.arrayHasElement(exceptLevels, lv) then
            table.insert(sorted, lv)
        end
    end
    local stringIndex = 1
    local string2index = {}
    local lvData = {}
    local placements = {}
    for _, lv in ipairs(sorted) do
        local entities = Workspace.GetExportEntities(lv)
        local placementId
        for _, data in ipairs(entities) do
            local prop = Helper.first(data.entityProperties, function(prop)
                return prop.name == "PlacementReference"
            end)
            local ref = prop.value.value
            placementId = ref.id
            local placementName = ref.type
            placements[placementName] = placements[placementName] or {}
            placements[placementName][placementId] = data.templateProperties
            for _, prop in ipairs(data.templateProperties) do
                if prop.value.type == FieldTypes.String then
                    local str = prop.value.value
                    if not string2index[str] then
                        string2index[str] = stringIndex
                        stringIndex = stringIndex + 1
                    end
                    prop.value.type = FieldTypes.Integer
                    prop.value.value = string2index[str]
                elseif prop.value.type == FieldTypes.StringArray then
                    local strArr = prop.value.value
                    local indices = {}
                    for _, str in ipairs(strArr) do
                        if not string2index[str] then
                            string2index[str] = stringIndex
                            stringIndex = stringIndex + 1
                        end
                        table.insert(indices, string2index[str])
                    end
                    prop.value.type = FieldTypes.IntegerArray
                    prop.value.value = indices
                end
            end
        end
        lvData[lv] = {
            placementId = placementId,
            entities = entities,
        }
    end
    -- remap ids
    local remappedPlacements = {}
    for name, placements in pairs(placements) do
        remappedPlacements[name] = remappedPlacements[name] or {}
        local newId = 1
        for id, props in pairs(placements) do
            remappedPlacements[name][id] = newId
            newId = newId + 1
        end
    end
    local placementVars, placementStructures = getPlacementVariants(placements, remappedPlacements)
    PlacementStructures = placementStructures
    PlacementRemap = remappedPlacements
    Helper.mergeArray(ret, placementVars)
    -- code generation
    local genFolder = Helper.combinePath(srcCodeFolder, "src", "game", "generated")
    local constHeaderFile = Helper.combinePath(genFolder, "ExportConstants.h")
    local constCppFile = Helper.combinePath(genFolder, "ExportConstants.cpp")
    local fConstHeader = io.open(constHeaderFile, "w")
    local fConstCpp = io.open(constCppFile, "w")
    Workspace.LogOptional("Generating " .. constHeaderFile)
    fConstHeader:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fConstHeader:write('#pragma once\n\n')
    fConstHeader:write('#include "core/core.h"\n')
    fConstHeader:write('#include "game/GameMacroes.h"\n')
    fConstHeader:write('#include <godot_cpp/godot.hpp>\n\n')
    fConstHeader:write('NS_GAME_BEGIN\n\n')
    fConstHeader:write('class ExportConstants {\n')
    fConstHeader:write('public:\n')
    fConstHeader:write('\tstatic const int GlobalUniformCount;\n')
    fConstHeader:write('\tstatic const int PlacementCounts[' .. #placementStructures .. '];\n')
    fConstHeader:write('};\n\n')
    fConstHeader:write('NS_GAME_ENDED')
    fConstHeader:flush()
    fConstHeader:close()
    Workspace.LogOptional("Generating " .. constCppFile)
    fConstCpp:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fConstCpp:write('#include "game/generated/ExportConstants.h"\n\n')
    fConstCpp:write('NS_GAME_BEGIN\n\n')
    fConstCpp:write('const int ExportConstants::GlobalUniformCount = ' .. #globalUniforms .. ';\n')
    fConstCpp:write('const int ExportConstants::PlacementCounts[' .. #placementStructures .. '] = {\n')
    for _, structure in ipairs(placementStructures) do
        fConstCpp:write('\t' .. tostring(Helper.count(placements[structure.name])) .. ',\n')
    end
    fConstCpp:write('};\n\n')
    fConstCpp:write('NS_GAME_ENDED')
    fConstCpp:flush()
    fConstCpp:close()
    for i, structure in ipairs(placementStructures) do
        local placementName = string.upper(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placement"
        local structHeaderFile = Helper.combinePath(genFolder, placementName .. ".h")
        local structCppFile = Helper.combinePath(genFolder, placementName .. ".cpp")
        local fStructHeader = io.open(structHeaderFile, "w")
        Workspace.LogOptional("Generating " .. structHeaderFile)
        fStructHeader:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
        fStructHeader:write('#pragma once\n\n')
        fStructHeader:write('#include "core/core.h"\n')
        fStructHeader:write('#include "game/GameMacroes.h"\n')
        fStructHeader:write('#include "game/field_types/FieldTypes.h"\n')
        fStructHeader:write('#include <godot_cpp/godot.hpp>\n')
        fStructHeader:write('#include <godot_cpp/classes/file_access.hpp>\n')
        fStructHeader:write('\n')
        fStructHeader:write('NS_GAME_BEGIN\n\n')
        fStructHeader:write('struct ' .. placementName .. ' {\n')
        fStructHeader:write('public:\n')
        fStructHeader:write('\tbool read(const godot::Ref<godot::FileAccess> &fs);\n\n')
        for _, field in ipairs(structure.fields) do
            local typeStr = ExportHelper.fieldType2CppType(field.type)
            if not typeStr then
                Workspace.ThrowException("Unsupported field type: " .. tostring(field.type))
            end
            local fieldName = "_" .. string.lower(string.sub(field.name, 1, 1)) .. string.sub(field.name, 2)
            fStructHeader:write(string.format('\tGETTER(%s, %s, %s);\n', fieldName, typeStr, field.name))
        end
        fStructHeader:write('};\n\n')
        fStructHeader:write('NS_GAME_ENDED')
        fStructHeader:flush()
        fStructHeader:close()
        local fStructCpp = io.open(structCppFile, "w")
        Workspace.LogOptional("Generating " .. structCppFile)
        fStructCpp:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
        fStructCpp:write('#include "game/generated/' .. placementName .. '.h"\n\n')
        fStructCpp:write('using namespace core;\n\n')
        fStructCpp:write('NS_GAME_BEGIN\n\n')
        fStructCpp:write('bool ' .. placementName .. '::read(const godot::Ref<godot::FileAccess> &fs) {\n')
        for _, field in ipairs(structure.fields) do
            local fieldName = "_" .. string.lower(string.sub(field.name, 1, 1)) .. string.sub(field.name, 2)
            if field.type >= FieldTypes.BooleanArray then
            else
                fStructCpp:write(string.format('\tif (!StorageHelper::readBinary(fs, %s)) return false;\n', fieldName))
            end
        end
        fStructCpp:write('\treturn true;\n')
        fStructCpp:write('}\n\n')
        fStructCpp:write('NS_GAME_ENDED')
        fStructCpp:flush()
        fStructCpp:close()
    end
    local gamedataHeaderFile = Helper.combinePath(genFolder, "GameData.h")
    local fGameDataHeader = io.open(gamedataHeaderFile, "w")
    Workspace.LogOptional("Generating " .. gamedataHeaderFile)
    fGameDataHeader:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fGameDataHeader:write('#pragma once\n\n')
    fGameDataHeader:write('#include "core/core.h"\n')
    fGameDataHeader:write('#include "game/GameMacroes.h"\n')
    for _, structure in ipairs(placementStructures) do
        local placementName = string.upper(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placement"
        fGameDataHeader:write('#include "game/generated/' .. placementName .. '.h"\n')
    end
    fGameDataHeader:write('#include <godot_cpp/godot.hpp>\n')
    fGameDataHeader:write('\n')
    fGameDataHeader:write('NS_GAME_BEGIN\n\n')
    fGameDataHeader:write('class GameData {\n')
    fGameDataHeader:write('\tfriend class GameDataLoader;\n')
    fGameDataHeader:write('public:\n')
    fGameDataHeader:write('\tFORCEINLINE const godot::Vector4 &globalUniformAt(u32 index) const { return _globalUniforms[index]; }\n')
    for _, structure in ipairs(placementStructures) do
        local placementName = string.upper(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placement"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placements"
        fGameDataHeader:write(string.format('\tFORCEINLINE const %s *get%s(u32 id) const { return &%s[id]; }\n', placementName, placementName, arrayName))
    end
    fGameDataHeader:write('\n')
    fGameDataHeader:write('private:\n')
    fGameDataHeader:write(string.format('\tgodot::Vector4 _globalUniforms[%d];\n', #globalUniforms))
    for _, structure in ipairs(placementStructures) do
        local placementName = string.upper(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placement"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placements"
        fGameDataHeader:write(string.format('\t%s %s[%d];\n', placementName, arrayName, Helper.count(placements[structure.name])))
    end
    fGameDataHeader:write('};\n\n')
    fGameDataHeader:write('NS_GAME_ENDED')
    fGameDataHeader:flush()
    fGameDataHeader:close()
    local loaderHeaderFile = Helper.combinePath(genFolder, "GameDataLoader.h")
    local loaderCppFile = Helper.combinePath(genFolder, "GameDataLoader.cpp")
    local fLoaderHeader = io.open(loaderHeaderFile, "w")
    Workspace.LogOptional("Generating " .. loaderHeaderFile)
    fLoaderHeader:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fLoaderHeader:write('#pragma once\n\n')
    fLoaderHeader:write('#include "core/core.h"\n')
    fLoaderHeader:write('#include "game/GameMacroes.h"\n')
    fLoaderHeader:write('#include "game/field_types/FieldTypes.h"\n')
    fLoaderHeader:write('#include "game/generated/GameData.h"\n')
    fLoaderHeader:write('#include "game/generated/ExportConstants.h"\n')
    for _, structure in ipairs(placementStructures) do
        local placementName = string.upper(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placement"
        fLoaderHeader:write('#include "game/generated/' .. placementName .. '.h"\n')
    end
    fLoaderHeader:write('#include <godot_cpp/godot.hpp>\n')
    fLoaderHeader:write('\n')
    fLoaderHeader:write('NS_GAME_BEGIN\n\n')
    fLoaderHeader:write('class GameDataLoader {\n')
    fLoaderHeader:write('public:\n')
    fLoaderHeader:write('\tbool load(GameData *data, const godot::String &dataPath);\n')
    fLoaderHeader:write('};\n\n')
    fLoaderHeader:write('NS_GAME_ENDED')
    fLoaderHeader:flush()
    fLoaderHeader:close()
    local fLoaderCpp = io.open(loaderCppFile, "w")
    Workspace.LogOptional("Generating " .. loaderCppFile)
    fLoaderCpp:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fLoaderCpp:write('#include "game/generated/GameDataLoader.h"\n')
    fLoaderCpp:write('#include <fstream>\n')
    fLoaderCpp:write('\n')
    fLoaderCpp:write('using namespace core;\n\n')
    fLoaderCpp:write('NS_GAME_BEGIN\n\n')
    fLoaderCpp:write('bool GameDataLoader::load(GameData *data, const godot::String &dataPath) {\n')
    fLoaderCpp:write('\tconst godot::Ref<godot::FileAccess> fs = godot::FileAccess::open(dataPath, godot::FileAccess::READ);\n')
    fLoaderCpp:write('\tif (!fs.is_valid()) return false;\n')
    fLoaderCpp:write('\tfor (int i = 0; i < ExportConstants::GlobalUniformCount; i++) {\n')
    fLoaderCpp:write('\t\tif (!StorageHelper::readBinary(fs, data->_globalUniforms[i])) return false;\n')
    fLoaderCpp:write('\t}\n')
    for i, structure in ipairs(placementStructures) do
        local placementName = string.upper(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placement"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placements"
        fLoaderCpp:write(string.format('\tfor (int i = 0; i < ExportConstants::PlacementCounts[%d]; i++) {\n', i - 1))
        fLoaderCpp:write(string.format('\t\t%s &placement = data->%s[i];\n', placementName, arrayName))
        fLoaderCpp:write('\t\tif (!placement.read(fs)) return false;\n')
        fLoaderCpp:write('\t}\n')
    end
    fLoaderCpp:write('\treturn true;\n')
    fLoaderCpp:write('}\n\n')
    fLoaderCpp:write('NS_GAME_ENDED\n')
    fLoaderCpp:flush()
    fLoaderCpp:close()
    return ret
end

function ExportSettings:beforeExportLevels(data, strMapping)
    local strHeaderFile = Helper.combinePath(data.SourceCodeFolder, "src", "game", "generated", "ExportedStrings.h")
    local strCppFile = Helper.combinePath(data.SourceCodeFolder, "src", "game", "generated", "ExportedStrings.cpp")
    local fStrHeader = io.open(strHeaderFile, "w")
    Workspace.LogOptional("Generating " .. strHeaderFile)
    local strArr = {}
    for str, index in pairs(strMapping) do
        strArr[index] = str
    end
    fStrHeader:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fStrHeader:write('#pragma once\n\n')
    fStrHeader:write('#include "core/core.h"\n')
    fStrHeader:write('#include "game/GameMacroes.h"\n')
    fStrHeader:write('#include <godot_cpp/godot.hpp>\n\n')
    fStrHeader:write('NS_GAME_BEGIN\n\n')
    fStrHeader:write('class ExportedStrings {\n')
    fStrHeader:write('public:\n')
    fStrHeader:write('\tstatic const char *AllStrings[' .. Helper.count(strArr) .. '];\n')
    fStrHeader:write('};\n\n')
    fStrHeader:write('NS_GAME_ENDED\n')
    fStrHeader:flush()
    fStrHeader:close()
    local fStrCpp = io.open(strCppFile, "w")
    Workspace.LogOptional("Generating " .. strCppFile)
    fStrCpp:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fStrCpp:write('#include "game/generated/ExportedStrings.h"\n\n')
    fStrCpp:write('NS_GAME_BEGIN\n\n')
    fStrCpp:write('const char *ExportedStrings::AllStrings[' .. Helper.count(strArr) .. '] = {\n')
    for _, str in ipairs(strArr) do
        fStrCpp:write(string.format('\t"%s",\n', str))
    end
    fStrCpp:write('};\n\n')
    fStrCpp:write('NS_GAME_ENDED\n')
    fStrCpp:flush()
    fStrCpp:close()
    local nameArr = Helper.select(PlacementStructures, function(structure)
        return structure.name
    end)
    local entityFactoryHeaderFile = Helper.combinePath(data.SourceCodeFolder, "src", "game", "generated", "EntityFactory.h")
    local entityFactoryCppFile = Helper.combinePath(data.SourceCodeFolder, "src", "game", "generated", "EntityFactory.cpp")
    local fEntityFactoryHeader = io.open(entityFactoryHeaderFile, "w")
    Workspace.LogOptional("Generating " .. entityFactoryHeaderFile)
    fEntityFactoryHeader:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fEntityFactoryHeader:write('#pragma once\n\n')
    fEntityFactoryHeader:write('#include "core/core.h"\n')
    fEntityFactoryHeader:write('#include "game/GameMacroes.h"\n')
    fEntityFactoryHeader:write('\n')
    fEntityFactoryHeader:write('NS_GAME_BEGIN\n\n')
    fEntityFactoryHeader:write('class EntityFactory {\n')
    fEntityFactoryHeader:write('public:\n')
    fEntityFactoryHeader:write('\tstatic core::DPtr<core::Entity2D> create(u32 placementTypeId, u32 placementId);\n')
    fEntityFactoryHeader:write('};\n\n')
    fEntityFactoryHeader:write('NS_GAME_ENDED\n')
    fEntityFactoryHeader:flush()
    fEntityFactoryHeader:close()
    local fEntityFactoryCpp = io.open(entityFactoryCppFile, "w")
    Workspace.LogOptional("Generating " .. entityFactoryCppFile)
    fEntityFactoryCpp:write('/// This file is generated by ExportSettings script, do not edit it manually.\n')
    fEntityFactoryCpp:write('#include "game/generated/EntityFactory.h"\n')
    for _, name in ipairs(nameArr) do
        fEntityFactoryCpp:write('#include "game/entities/' .. name .. '.h"\n')
    end
    fEntityFactoryCpp:write('\n')
    fEntityFactoryCpp:write('NS_GAME_BEGIN\n\n')
    fEntityFactoryCpp:write('core::DPtr<core::Entity2D> EntityFactory::create(u32 placementTypeId, u32 placementId) {\n')
    fEntityFactoryCpp:write('\tswitch (placementTypeId) {\n')
    for i, name in ipairs(nameArr) do
        fEntityFactoryCpp:write(string.format('\t\tcase %d: return DPTR(%s, placementId);\n', i - 1, name))
    end
    fEntityFactoryCpp:write('\t\tdefault: return nullptr;\n')
    fEntityFactoryCpp:write('\t}\n')
    fEntityFactoryCpp:write('}\n\n')
    fEntityFactoryCpp:write('NS_GAME_ENDED\n')
    fEntityFactoryCpp:flush()
    fEntityFactoryCpp:close()
    local remapArr = Helper.select(nameArr, function(name)
        return {
            name = name,
            remap = PlacementRemap[name],
        }
    end)
    return remapArr
end

return ExportSettings