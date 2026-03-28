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

local function toCppName(value)
    return string.upper(string.sub(value, 1, 1)) .. string.sub(value, 2)
end

local function sortedKeys(input)
    local keys = {}
    for key, _ in pairs(input) do
        table.insert(keys, key)
    end
    table.sort(keys)
    return keys
end

local function getSortedGameDataGroups(gamedataByType)
    local groups = {}
    local groupMap = {}
    for _, info in pairs(gamedataByType) do
        if not groupMap[info.group] then
            groupMap[info.group] = true
            table.insert(groups, info.group)
        end
    end
    table.sort(groups)
    return groups
end

local function getGlobalUniformVariants(uniforms)
    local ret = {}
    for _, uniform in ipairs(PipelineHook.uniforms) do
        if Helper.arrayHasElement(uniforms, uniform.name) then
            local value = uniform.value
            if uniform.type == PipelineHookUniformTypes.Float1 then
                value = { x = value.x, y = 0.0, z = 0.0, w = 0.0, }
                table.insert(ret, {
                    name = uniform.name,
                    value = {
                        type = FieldTypes.Color,
                        value = value,
                    },
                })
            elseif uniform.type == PipelineHookUniformTypes.Float2 then
                value = { x = value.x, y = value.y, z = 0.0, w = 0.0, }
                table.insert(ret, {
                    name = uniform.name,
                    value = {
                        type = FieldTypes.Color,
                        value = value,
                    },
                })
            elseif uniform.type == PipelineHookUniformTypes.Float3 then
                value = { x = value.x, y = value.y, z = value.z, w = 0.0, }
                table.insert(ret, {
                    name = uniform.name,
                    value = {
                        type = FieldTypes.Color,
                        value = value,
                    },
                })
            elseif uniform.type == PipelineHookUniformTypes.Float4 then
                table.insert(ret, {
                    name = uniform.name,
                    value = {
                        type = FieldTypes.Color,
                        value = value,
                    },
                })
            end
        end
    end
    return ret
end

local function buildPlacementRemap(placements)
    local remap = {}
    for typeName, typedPlacements in pairs(placements) do
        local ids = sortedKeys(typedPlacements)
        remap[typeName] = {}
        local newId = 0
        for _, oldId in ipairs(ids) do
            remap[typeName][oldId] = newId
            newId = newId + 1
        end
    end
    return remap
end

local function buildGameDataRemap(gamedataByType)
    local groupRemap = {}
    local typeRemap = {}
    local groupTypeMap = {}

    local groupedIds = {}
    for _, info in pairs(gamedataByType) do
        groupedIds[info.group] = groupedIds[info.group] or {}
        for id, _ in pairs(info.objects) do
            groupedIds[info.group][id] = true
        end
    end

    for group, idsMap in pairs(groupedIds) do
        local ids = sortedKeys(idsMap)
        groupRemap[group] = {}
        groupTypeMap[group] = {}
        local newId = 0
        for _, oldId in ipairs(ids) do
            groupRemap[group][oldId] = newId
            newId = newId + 1
        end
    end

    for typeName, info in pairs(gamedataByType) do
        local gRemap = groupRemap[info.group] or {}
        typeRemap[typeName] = {}
        for id, _ in pairs(info.objects) do
            if gRemap[id] ~= nil then
                local newId = gRemap[id]
                typeRemap[typeName][id] = newId
                groupTypeMap[info.group][newId] = typeName
            end
        end
    end

    return typeRemap, groupRemap, groupTypeMap
end

local function remapTypeReference(ref, placementRemap, gamedataTypeRemap)
    if not ref then
        return
    end
    local remap = placementRemap[ref.type]
    if not remap then
        remap = gamedataTypeRemap[ref.type]
    end
    if remap and remap[ref.id] ~= nil then
        ref.id = remap[ref.id]
    end
end

local function remapGroupReference(ref, gamedataGroupRemap)
    if not ref then
        return
    end
    local remap = gamedataGroupRemap[ref.group]
    if remap and remap[ref.id] ~= nil then
        ref.id = remap[ref.id]
    end
end

local function remapVariantReference(variant, placementRemap, gamedataTypeRemap, gamedataGroupRemap)
    if variant.type == FieldTypes.TypeReference then
        remapTypeReference(variant.value, placementRemap, gamedataTypeRemap)
    elseif variant.type == FieldTypes.TypeReferenceArray then
        for _, ref in ipairs(variant.value) do
            remapTypeReference(ref, placementRemap, gamedataTypeRemap)
        end
    elseif variant.type == FieldTypes.GroupReference then
        remapGroupReference(variant.value, gamedataGroupRemap)
    elseif variant.type == FieldTypes.GroupReferenceArray then
        for _, ref in ipairs(variant.value) do
            remapGroupReference(ref, gamedataGroupRemap)
        end
    end
end

local function remapPropertyReferences(props, placementRemap, gamedataTypeRemap, gamedataGroupRemap)
    for _, prop in ipairs(props) do
        remapVariantReference(prop.value, placementRemap, gamedataTypeRemap, gamedataGroupRemap)
    end
end

local function getPlacementVariants(placements)
    local ret = {}
    local structures = {}
    for name, placement in pairs(placements) do
        local fields = {}
        for _, props in pairs(placement) do
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
            count = Helper.count(placement),
        })
    end
    return ret, structures
end

local function getGameDataVariants(gamedataMap)
    local ret = {}
    local structures = {}
    for typeName, info in pairs(gamedataMap) do
        local fields = {}
        for _, props in pairs(info.objects) do
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
            name = typeName,
            group = info.group,
            fields = fields,
            count = Helper.count(info.objects),
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
local GameDataStructures = nil
local GameDataTypeRemap = nil
local GameDataGroupRemap = nil
local GameDataGroupTypeMap = nil
local GameDataGroups = nil

function ExportSettings:gamedataVariants(data)
    local srcCodeFolder = data.SourceCodeFolder
    local globalUniforms = data.globalUniforms
    local exceptLevels = data.exceptLevels
    local allLvs = Workspace.AllLevelNames
    if #allLvs == 0 then
        Workspace.ThrowException("No levels found in the workspace.")
    end

    local ret = {}
    local uniforms = getGlobalUniformVariants(globalUniforms)
    Helper.mergeArray(ret, Helper.select(uniforms, function(uniform)
        return uniform.value
    end))

    local sorted = {}
    for _, lv in ipairs(allLvs) do
        if not Helper.arrayHasElement(exceptLevels, lv) then
            table.insert(sorted, lv)
        end
    end

    local placements = {}
    for _, lv in ipairs(sorted) do
        local entities = Workspace.GetExportEntities(lv)
        for _, entity in ipairs(entities) do
            local prop = Helper.first(entity.entityProperties, function(p)
                return p.name == "PlacementReference"
            end)
            local ref = prop.value.value
            local placementName = ref.type
            local placementId = ref.id
            placements[placementName] = placements[placementName] or {}
            placements[placementName][placementId] = entity.templateProperties
        end
    end

    local gamedataByType = {}
    local exportGameData = Workspace.GetExportGameData()
    -- NOTE: gd.group (GroupName) is only used to organize types into groups for mapping.
    --       The group value itself should NOT be exported as a string resource,
    --       since C++ uses GroupTypeMap for type/group resolution instead.
    for _, gd in ipairs(exportGameData) do
        local typeName = gd.type
        local groupName = gd.group
        local props = gd.properties
        local idProp = Helper.first(props, function(prop)
            return prop.name == "id"
        end)
        if idProp then
            local oldId = idProp.value.value
            gamedataByType[typeName] = gamedataByType[typeName] or {
                group = groupName,
                objects = {},
            }
            if gamedataByType[typeName].group ~= groupName then
                Workspace.ThrowException("GameData type '" .. typeName .. "' is bound to multiple groups.")
            end
            gamedataByType[typeName].objects[oldId] = props
        end
    end

    local remappedPlacements = buildPlacementRemap(placements)
    local remappedGameDataType, remappedGameDataGroup, remappedGameDataGroupTypeMap = buildGameDataRemap(gamedataByType)

    for typeName, typedPlacements in pairs(placements) do
        for oldId, props in pairs(typedPlacements) do
            local idProp = Helper.first(props, function(prop)
                return prop.name == "id"
            end)
            if idProp and remappedPlacements[typeName] and remappedPlacements[typeName][oldId] ~= nil then
                idProp.value.value = remappedPlacements[typeName][oldId]
            end
            remapPropertyReferences(props, remappedPlacements, remappedGameDataType, remappedGameDataGroup)
        end
    end

    for typeName, info in pairs(gamedataByType) do
        for oldId, props in pairs(info.objects) do
            local idProp = Helper.first(props, function(prop)
                return prop.name == "id"
            end)
            if idProp and remappedGameDataType[typeName] and remappedGameDataType[typeName][oldId] ~= nil then
                idProp.value.value = remappedGameDataType[typeName][oldId]
            end
            remapPropertyReferences(props, remappedPlacements, remappedGameDataType, remappedGameDataGroup)
        end
    end

    local placementVars, placementStructures = getPlacementVariants(placements)
    local gamedataVars, gamedataStructures = getGameDataVariants(gamedataByType)
    local gameDataGroups = getSortedGameDataGroups(gamedataByType)

    PlacementStructures = placementStructures
    PlacementRemap = remappedPlacements
    GameDataStructures = gamedataStructures
    GameDataTypeRemap = remappedGameDataType
    GameDataGroupRemap = remappedGameDataGroup
    GameDataGroupTypeMap = remappedGameDataGroupTypeMap
    GameDataGroups = gameDataGroups

    Helper.mergeArray(ret, placementVars)
    Helper.mergeArray(ret, gamedataVars)

    local genFolder = Helper.combinePath(srcCodeFolder, "src", "game", "generated")
    Workspace.Log("Clearing " .. genFolder)
    Workspace.ClearDirectory(genFolder)
    local constHeaderFile = Helper.combinePath(genFolder, "ExportConstants.h")
    local constCppFile = Helper.combinePath(genFolder, "ExportConstants.cpp")
    local fConstHeader = io.open(constHeaderFile, "w")
    local fConstCpp = io.open(constCppFile, "w")
    Workspace.Log("Generating " .. constHeaderFile)
    fConstHeader:write('/// This file is generated by Fantania, do not edit it manually.\n')
    fConstHeader:write('#pragma once\n\n')
    fConstHeader:write('#include "core/core.h"\n')
    fConstHeader:write('#include "game/GameMacroes.h"\n')
    fConstHeader:write('#include <godot_cpp/godot.hpp>\n\n')
    fConstHeader:write('NS_GAME_BEGIN\n\n')
    fConstHeader:write('class ExportConstants {\n')
    fConstHeader:write('public:\n')
    
    local typeEnumNames = {}
    local groupEnumNames = {}
    fConstHeader:write('\tenum class GameDataTypeEnum : int {\n')
    for i, structure in ipairs(gamedataStructures) do
        local typeName = toCppName(structure.name) .. "Config"
        typeEnumNames[structure.name] = typeName
        fConstHeader:write(string.format('\t\t%s = %d,\n', typeName, i - 1))
    end
    fConstHeader:write('\t};\n\n')
    fConstHeader:write('\tenum class GameDataGroupEnum : int {\n')
    for i, group in ipairs(gameDataGroups) do
        local groupName = toCppName(group)
        groupEnumNames[group] = groupName
        fConstHeader:write(string.format('\t\t%s = %d,\n', groupName, i - 1))
    end
    fConstHeader:write('\t};\n\n')
    
    fConstHeader:write('\tstatic const int GlobalUniformCount;\n')
    if #placementStructures > 0 then
        fConstHeader:write('\tstatic const int PlacementCounts[' .. #placementStructures .. '];\n')
    end
    if #gamedataStructures > 0 then
        fConstHeader:write('\tstatic const int GameDataCounts[' .. #gamedataStructures .. '];\n')
    end
    
    for group, typeMap in pairs(remappedGameDataGroupTypeMap) do
        local groupCount = Helper.count(typeMap)
        fConstHeader:write('\tstatic const GameDataTypeEnum GroupTypeMap_' .. toCppName(group) .. '[' .. groupCount .. '];\n')
    end
    
    fConstHeader:write('};\n\n')
    fConstHeader:write('NS_GAME_ENDED')
    fConstHeader:flush()
    fConstHeader:close()

    Workspace.Log("Generating " .. constCppFile)
    fConstCpp:write('/// This file is generated by Fantania, do not edit it manually.\n')
    fConstCpp:write('#include "game/generated/ExportConstants.h"\n\n')
    fConstCpp:write('NS_GAME_BEGIN\n\n')
    fConstCpp:write('const int ExportConstants::GlobalUniformCount = ' .. #globalUniforms .. ';\n')
    if #placementStructures > 0 then
        fConstCpp:write('const int ExportConstants::PlacementCounts[' .. #placementStructures .. '] = {\n')
        for _, structure in ipairs(placementStructures) do
            fConstCpp:write('\t' .. tostring(structure.count) .. ',\n')
        end
        fConstCpp:write('};\n')
    end
    if #gamedataStructures > 0 then
        fConstCpp:write('const int ExportConstants::GameDataCounts[' .. #gamedataStructures .. '] = {\n')
        for _, structure in ipairs(gamedataStructures) do
            fConstCpp:write('\t' .. tostring(structure.count) .. ',\n')
        end
        fConstCpp:write('};\n')
    end
    fConstCpp:write('\n')
    
    for group, typeMap in pairs(remappedGameDataGroupTypeMap) do
        local groupCount = Helper.count(typeMap)
        fConstCpp:write('const ExportConstants::GameDataTypeEnum ExportConstants::GroupTypeMap_' .. toCppName(group) .. '[' .. groupCount .. '] = {\n')
        for newId = 0, groupCount - 1 do
            local typeName = typeMap[newId]
            local enumName = typeEnumNames[typeName]
            fConstCpp:write(string.format('\tExportConstants::GameDataTypeEnum::%s,\n', enumName))
        end
        fConstCpp:write('};\n\n')
    end
    
    fConstCpp:write('NS_GAME_ENDED')
    fConstCpp:flush()
    fConstCpp:close()

    for _, structure in ipairs(placementStructures) do
        local placementName = toCppName(structure.name) .. "Placement"
        local structHeaderFile = Helper.combinePath(genFolder, placementName .. ".h")
        local structCppFile = Helper.combinePath(genFolder, placementName .. ".cpp")
        local fStructHeader = io.open(structHeaderFile, "w")
        Workspace.Log("Generating " .. structHeaderFile)
        fStructHeader:write('/// This file is generated by Fantania, do not edit it manually.\n')
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
        Workspace.Log("Generating " .. structCppFile)
        fStructCpp:write('/// This file is generated by Fantania, do not edit it manually.\n')
        fStructCpp:write('#include "game/miscs/TemplateSpecializations.h"\n')
        fStructCpp:write('#include "game/generated/' .. placementName .. '.h"\n\n')
        fStructCpp:write('using namespace core;\n\n')
        fStructCpp:write('NS_GAME_BEGIN\n\n')
        fStructCpp:write('bool ' .. placementName .. '::read(const godot::Ref<godot::FileAccess> &fs) {\n')
        for _, field in ipairs(structure.fields) do
            local fieldName = "_" .. string.lower(string.sub(field.name, 1, 1)) .. string.sub(field.name, 2)
            fStructCpp:write(string.format('\tif (!StorageHelper::readBinary(fs, %s)) return false;\n', fieldName))
        end
        fStructCpp:write('\treturn true;\n')
        fStructCpp:write('}\n\n')
        fStructCpp:write('NS_GAME_ENDED')
        fStructCpp:flush()
        fStructCpp:close()
    end

    for _, structure in ipairs(gamedataStructures) do
        local dataName = toCppName(structure.name) .. "Config"
        local dataHeaderFile = Helper.combinePath(genFolder, dataName .. ".h")
        local dataCppFile = Helper.combinePath(genFolder, dataName .. ".cpp")

        local fDataHeader = io.open(dataHeaderFile, "w")
        Workspace.Log("Generating " .. dataHeaderFile)
        fDataHeader:write('/// This file is generated by Fantania, do not edit it manually.\n')
        fDataHeader:write('#pragma once\n\n')
        fDataHeader:write('#include "core/core.h"\n')
        fDataHeader:write('#include "game/GameMacroes.h"\n')
        fDataHeader:write('#include "game/field_types/FieldTypes.h"\n')
        fDataHeader:write('#include <godot_cpp/godot.hpp>\n')
        fDataHeader:write('#include <godot_cpp/classes/file_access.hpp>\n\n')
        fDataHeader:write('NS_GAME_BEGIN\n\n')
        fDataHeader:write('struct ' .. dataName .. ' {\n')
        fDataHeader:write('public:\n')
        fDataHeader:write('\tbool read(const godot::Ref<godot::FileAccess> &fs);\n\n')
        for _, field in ipairs(structure.fields) do
            local typeStr = ExportHelper.fieldType2CppType(field.type)
            if not typeStr then
                Workspace.ThrowException("Unsupported field type: " .. tostring(field.type))
            end
            local fieldName = "_" .. string.lower(string.sub(field.name, 1, 1)) .. string.sub(field.name, 2)
            fDataHeader:write(string.format('\tGETTER(%s, %s, %s);\n', fieldName, typeStr, field.name))
        end
        fDataHeader:write('};\n\n')
        fDataHeader:write('NS_GAME_ENDED')
        fDataHeader:flush()
        fDataHeader:close()

        local fDataCpp = io.open(dataCppFile, "w")
        Workspace.Log("Generating " .. dataCppFile)
        fDataCpp:write('/// This file is generated by Fantania, do not edit it manually.\n')
        fDataCpp:write('#include "game/miscs/TemplateSpecializations.h"\n')
        fDataCpp:write('#include "game/generated/' .. dataName .. '.h"\n\n')
        fDataCpp:write('using namespace core;\n\n')
        fDataCpp:write('NS_GAME_BEGIN\n\n')
        fDataCpp:write('bool ' .. dataName .. '::read(const godot::Ref<godot::FileAccess> &fs) {\n')
        for _, field in ipairs(structure.fields) do
            local fieldName = "_" .. string.lower(string.sub(field.name, 1, 1)) .. string.sub(field.name, 2)
            fDataCpp:write(string.format('\tif (!StorageHelper::readBinary(fs, %s)) return false;\n', fieldName))
        end
        fDataCpp:write('\treturn true;\n')
        fDataCpp:write('}\n\n')
        fDataCpp:write('NS_GAME_ENDED')
        fDataCpp:flush()
        fDataCpp:close()
    end

    local gamedataHeaderFile = Helper.combinePath(genFolder, "GameData.h")
    local gamedataCppFile = Helper.combinePath(genFolder, "GameData.cpp")
    local fGameDataHeader = io.open(gamedataHeaderFile, "w")
    Workspace.Log("Generating " .. gamedataHeaderFile)
    fGameDataHeader:write('/// This file is generated by Fantania, do not edit it manually.\n')
    fGameDataHeader:write('#pragma once\n\n')
    fGameDataHeader:write('#include "core/core.h"\n')
    fGameDataHeader:write('#include "game/GameMacroes.h"\n')
    for _, structure in ipairs(placementStructures) do
        local placementName = toCppName(structure.name) .. "Placement"
        fGameDataHeader:write('#include "game/generated/' .. placementName .. '.h"\n')
    end
    for _, structure in ipairs(gamedataStructures) do
        local dataName = toCppName(structure.name) .. "Config"
        fGameDataHeader:write('#include "game/generated/' .. dataName .. '.h"\n')
    end
    fGameDataHeader:write('#include <godot_cpp/godot.hpp>\n')
    fGameDataHeader:write('\n')
    fGameDataHeader:write('#define GLOBAL_UNIFORM_COUNT ' .. #globalUniforms .. '\n\n')
    fGameDataHeader:write('NS_GAME_BEGIN\n\n')
    fGameDataHeader:write('class GameData {\n')
    fGameDataHeader:write('\tfriend class GameDataLoader;\n')
    fGameDataHeader:write('public:\n')
    fGameDataHeader:write('\tstatic const char *UniformNames[GLOBAL_UNIFORM_COUNT];\n')
    fGameDataHeader:write('\tFORCEINLINE const godot::Vector4 &globalUniformAt(u32 index) const { return _globalUniforms[index]; }\n')
    for _, structure in ipairs(placementStructures) do
        local placementName = toCppName(structure.name) .. "Placement"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placements"
        fGameDataHeader:write(string.format('\tFORCEINLINE const %s *get%s(u32 id) const { return &%s[id]; }\n', placementName, placementName, arrayName))
    end
    fGameDataHeader:write('\n')
    for _, structure in ipairs(gamedataStructures) do
        local dataName = toCppName(structure.name) .. "Config"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Config"
        fGameDataHeader:write(string.format('\tFORCEINLINE const %s *get%s(u32 id) const { return &%s[id]; }\n', dataName, dataName, arrayName))
    end
    fGameDataHeader:write('\n')
    fGameDataHeader:write('private:\n')
    fGameDataHeader:write('\tgodot::Vector4 _globalUniforms[GLOBAL_UNIFORM_COUNT];\n')
    for _, structure in ipairs(placementStructures) do
        local placementName = toCppName(structure.name) .. "Placement"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placements"
        fGameDataHeader:write(string.format('\t%s %s[%d];\n', placementName, arrayName, structure.count))
    end
    fGameDataHeader:write('\n')
    for _, structure in ipairs(gamedataStructures) do
        local dataName = toCppName(structure.name) .. "Config"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Config"
        fGameDataHeader:write(string.format('\t%s %s[%d];\n', dataName, arrayName, structure.count))
    end
    fGameDataHeader:write('};\n\n')
    fGameDataHeader:write('NS_GAME_ENDED')
    fGameDataHeader:flush()
    fGameDataHeader:close()

    local fGameDataCpp = io.open(gamedataCppFile, "w")
    Workspace.Log("Generating " .. gamedataCppFile)
    fGameDataCpp:write('/// This file is generated by Fantania, do not edit it manually.\n')
    fGameDataCpp:write('#include "game/generated/GameData.h"\n\n')
    fGameDataCpp:write('using namespace core;\n\n')
    fGameDataCpp:write('NS_GAME_BEGIN\n\n')
    fGameDataCpp:write('const char *GameData::UniformNames[GLOBAL_UNIFORM_COUNT] = {\n')
    for _, uniform in ipairs(uniforms) do
        fGameDataCpp:write(string.format('\t"%s",\n', uniform.name))
    end
    fGameDataCpp:write('};\n\n')
    fGameDataCpp:write('NS_GAME_ENDED\n')
    fGameDataCpp:flush()
    fGameDataCpp:close()

    local loaderHeaderFile = Helper.combinePath(genFolder, "GameDataLoader.h")
    local loaderCppFile = Helper.combinePath(genFolder, "GameDataLoader.cpp")
    local fLoaderHeader = io.open(loaderHeaderFile, "w")
    Workspace.Log("Generating " .. loaderHeaderFile)
    fLoaderHeader:write('/// This file is generated by Fantania, do not edit it manually.\n')
    fLoaderHeader:write('#pragma once\n\n')
    fLoaderHeader:write('#include "core/core.h"\n')
    fLoaderHeader:write('#include "game/GameMacroes.h"\n')
    fLoaderHeader:write('#include "game/field_types/FieldTypes.h"\n')
    fLoaderHeader:write('#include "game/generated/GameData.h"\n')
    fLoaderHeader:write('#include "game/generated/ExportConstants.h"\n')
    for _, structure in ipairs(placementStructures) do
        local placementName = toCppName(structure.name) .. "Placement"
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
    Workspace.Log("Generating " .. loaderCppFile)
    fLoaderCpp:write('/// This file is generated by Fantania, do not edit it manually.\n')
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
        local placementName = toCppName(structure.name) .. "Placement"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Placements"
        fLoaderCpp:write(string.format('\tfor (int i = 0; i < ExportConstants::PlacementCounts[%d]; i++) {\n', i - 1))
        fLoaderCpp:write(string.format('\t\t%s &placement = data->%s[i];\n', placementName, arrayName))
        fLoaderCpp:write('\t\tif (!placement.read(fs)) return false;\n')
        fLoaderCpp:write('\t}\n')
    end
    for i, structure in ipairs(gamedataStructures) do
        local dataName = toCppName(structure.name) .. "Config"
        local arrayName = "_" .. string.lower(string.sub(structure.name, 1, 1)) .. string.sub(structure.name, 2) .. "Config"
        fLoaderCpp:write(string.format('\tfor (int i = 0; i < ExportConstants::GameDataCounts[%d]; i++) {\n', i - 1))
        fLoaderCpp:write(string.format('\t\t%s &item = data->%s[i];\n', dataName, arrayName))
        fLoaderCpp:write('\t\tif (!item.read(fs)) return false;\n')
        fLoaderCpp:write('\t}\n')
    end
    fLoaderCpp:write('\treturn true;\n')
    fLoaderCpp:write('}\n\n')
    fLoaderCpp:write('NS_GAME_ENDED\n')
    fLoaderCpp:flush()
    fLoaderCpp:close()

    return ret
end

function ExportSettings:exportedGameDataGroups()
    return GameDataGroups or {}
end

function ExportSettings:beforeExportLevels(data, strMapping)
    local strHeaderFile = Helper.combinePath(data.SourceCodeFolder, "src", "game", "generated", "ExportedStrings.h")
    local strCppFile = Helper.combinePath(data.SourceCodeFolder, "src", "game", "generated", "ExportedStrings.cpp")
    local fStrHeader = io.open(strHeaderFile, "w")
    Workspace.Log("Generating " .. strHeaderFile)
    local strArr = {}
    for str, index in pairs(strMapping) do
        strArr[index + 1] = str
    end
    fStrHeader:write('/// This file is generated by Fantania, do not edit it manually.\n')
    fStrHeader:write('#pragma once\n\n')
    fStrHeader:write('#include "core/core.h"\n')
    fStrHeader:write('#include "game/GameMacroes.h"\n')
    fStrHeader:write('#include <godot_cpp/godot.hpp>\n\n')
    fStrHeader:write('#define EXPORTED_STRING_COUNT ' .. #strArr .. '\n\n')
    fStrHeader:write('NS_GAME_BEGIN\n\n')
    fStrHeader:write('class ExportedStrings {\n')
    fStrHeader:write('public:\n')
    fStrHeader:write('\tstatic const char *AllStrings[EXPORTED_STRING_COUNT];\n')
    fStrHeader:write('};\n\n')
    fStrHeader:write('NS_GAME_ENDED\n')
    fStrHeader:flush()
    fStrHeader:close()

    local fStrCpp = io.open(strCppFile, "w")
    Workspace.Log("Generating " .. strCppFile)
    fStrCpp:write('/// This file is generated by Fantania, do not edit it manually.\n')
    fStrCpp:write('#include "game/generated/ExportedStrings.h"\n\n')
    fStrCpp:write('NS_GAME_BEGIN\n\n')
    fStrCpp:write('const char *ExportedStrings::AllStrings[EXPORTED_STRING_COUNT] = {\n')
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
    Workspace.Log("Generating " .. entityFactoryHeaderFile)
    fEntityFactoryHeader:write('/// This file is generated by Fantania, do not edit it manually.\n')
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
    Workspace.Log("Generating " .. entityFactoryCppFile)
    fEntityFactoryCpp:write('/// This file is generated by Fantania, do not edit it manually.\n')
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
