-- Lua 通用 Dump 工具
-- 支持：table、function、userdata、thread、nil、boolean、number、string
-- 特性：循环引用检测、缩进格式化、最大深度限制、自定义输出

local Dump = {}

-- 默认配置
Dump.config = {
    indent = "  ",          -- 缩进字符串
    maxDepth = 10,          -- 最大递归深度
    showMetatable = true,   -- 是否显示元表
    showFunctions = true,   -- 是否显示函数信息
    arrayDetection = true,  -- 是否自动检测数组部分
    sortKeys = false,       -- 是否按键排序
    pretty = true,          -- 是否美化输出
}

-- 内部状态管理
local visited = {}
local currentDepth = 0

-- 主要 Dump 函数
function Dump.dump(value, options)
    -- 重置状态
    visited = {}
    currentDepth = 0
    
    -- 合并配置
    local config = setmetatable(options or {}, { __index = Dump.config })
    
    -- 生成输出
    local result = Dump._dumpValue(value, config, "")
    
    -- 清理状态
    visited = {}
    return result
end

-- 递归处理值
function Dump._dumpValue(value, config, prefix)
    local valueType = type(value)
    
    -- 处理 nil
    if value == nil then
        return "nil"
    end
    
    -- 处理布尔值
    if valueType == "boolean" then
        return tostring(value)
    end
    
    -- 处理数字
    if valueType == "number" then
        -- 检查是否为整数
        if math.floor(value) == value then
            return tostring(value)
        else
            return string.format("%.6f", value)
        end
    end
    
    -- 处理字符串
    if valueType == "string" then
        -- 转义特殊字符
        local escaped = value:gsub("\\", "\\\\")
                           :gsub("\"", "\\\"")
                           :gsub("\n", "\\n")
                           :gsub("\r", "\\r")
                           :gsub("\t", "\\t")
        return string.format("\"%s\"", escaped)
    end
    
    -- 处理函数
    if valueType == "function" then
        if config.showFunctions then
            -- 尝试获取函数信息
            local info = debug.getinfo(value, "nS")
            local name = info.name or "<anonymous>"
            local source = info.source or "<unknown>"
            local linedefined = info.linedefined or 0
            return string.format("function: %s (defined at %s:%d)", 
                name, source, linedefined)
        else
            return "function"
        end
    end
    
    -- 处理线程
    if valueType == "thread" then
        return string.format("thread: %s", tostring(value))
    end
    
    -- 处理 userdata
    if valueType == "userdata" then
        -- 尝试获取元表类型
        local mt = getmetatable(value)
        if mt and mt.__type then
            return string.format("userdata<%s>: %s", mt.__type, tostring(value))
        else
            return string.format("userdata: %s", tostring(value))
        end
    end
    
    -- 处理表（最复杂的情况）
    if valueType == "table" then
        return Dump._dumpTable(value, config, prefix)
    end
    
    -- 未知类型
    return string.format("unknown<%s>: %s", valueType, tostring(value))
end

-- 处理表
function Dump._dumpTable(tbl, config, prefix)
    -- 检查循环引用
    if visited[tbl] then
        return "<circular reference>"
    end
    
    -- 检查深度限制
    currentDepth = currentDepth + 1
    if currentDepth > config.maxDepth then
        currentDepth = currentDepth - 1
        return "<max depth reached>"
    end
    
    -- 标记为已访问
    visited[tbl] = true
    
    -- 检查是否为数组
    local isArray, arrayPart, mapPart = Dump._splitArrayAndMap(tbl, config)
    
    local result = {}
    local newPrefix = prefix .. config.indent
    
    -- 处理数组部分
    if isArray and #arrayPart > 0 then
        table.insert(result, "{")
        
        for i, v in ipairs(arrayPart) do
            local itemStr = Dump._dumpValue(v, config, newPrefix)
            if config.pretty then
                table.insert(result, string.format("%s%s,", newPrefix, itemStr))
            else
                table.insert(result, itemStr .. ",")
            end
        end
        
        -- 如果有映射部分，添加分隔符
        if mapPart and next(mapPart) then
            if config.pretty then
                table.insert(result, newPrefix .. "-- map part --")
            end
        end
    else
        table.insert(result, "{")
    end
    
    -- 处理映射部分（非数组键值对）
    if mapPart then
        -- 获取并排序键
        local keys = {}
        for k in pairs(mapPart) do
            table.insert(keys, k)
        end
        
        if config.sortKeys then
            table.sort(keys, function(a, b)
                local ta, tb = type(a), type(b)
                if ta == tb then
                    return tostring(a) < tostring(b)
                end
                return ta < tb  -- 按类型排序
            end)
        end
        
        for _, k in ipairs(keys) do
            local v = mapPart[k]
            local keyStr
            local valueStr = Dump._dumpValue(v, config, newPrefix)
            
            -- 格式化键
            if type(k) == "string" and k:match("^[%a_][%w_]*$") then
                -- 有效的标识符
                keyStr = k
            elseif type(k) == "string" then
                -- 需要引号的字符串键
                keyStr = string.format("[%q]", k)
            elseif type(k) == "number" then
                -- 数字键
                keyStr = string.format("[%s]", tostring(k))
            else
                -- 其他类型键
                keyStr = string.format("[%s]", Dump._dumpValue(k, config, ""))
            end
            
            if config.pretty then
                table.insert(result, string.format("%s%s = %s,", newPrefix, keyStr, valueStr))
            else
                table.insert(result, string.format("%s = %s,", keyStr, valueStr))
            end
        end
    end
    
    -- 处理元表
    if config.showMetatable then
        local mt = getmetatable(tbl)
        if mt then
            local mtStr = Dump._dumpValue(mt, config, newPrefix)
            if config.pretty then
                table.insert(result, string.format("%s__metatable = %s,", newPrefix, mtStr))
            else
                table.insert(result, string.format("__metatable = %s,", mtStr))
            end
        end
    end
    
    -- 闭合大括号
    if config.pretty then
        if #result > 1 then
            table.insert(result, prefix .. "}")
        else
            result[1] = "{}"
        end
    else
        if #result > 1 then
            table.insert(result, "}")
        else
            result[1] = "{}"
        end
    end
    
    -- 恢复状态
    currentDepth = currentDepth - 1
    visited[tbl] = nil
    
    if config.pretty then
        return table.concat(result, "\n")
    else
        return table.concat(result, " ")
    end
end

-- 分离数组部分和映射部分
function Dump._splitArrayAndMap(tbl, config)
    if not config.arrayDetection then
        return false, {}, tbl
    end
    
    local arrayPart = {}
    local mapPart = {}
    
    -- 计算最大连续整数键
    local maxIndex = 0
    for k, v in pairs(tbl) do
        if type(k) == "number" and k > 0 and math.floor(k) == k then
            if k > maxIndex then
                maxIndex = k
            end
            arrayPart[k] = v
        else
            mapPart[k] = v
        end
    end
    
    -- 检查是否是连续数组
    local isArray = true
    for i = 1, maxIndex do
        if arrayPart[i] == nil then
            isArray = false
            -- 将缺失的键也移到映射部分
            if arrayPart[i] ~= nil then
                mapPart[i] = arrayPart[i]
                arrayPart[i] = nil
            end
        end
    end
    
    -- 提取连续部分
    local continuousArray = {}
    for i = 1, maxIndex do
        if arrayPart[i] ~= nil then
            table.insert(continuousArray, arrayPart[i])
        else
            break
        end
    end
    
    -- 将剩余的数字键移到映射部分
    for k, v in pairs(arrayPart) do
        if k > #continuousArray then
            mapPart[k] = v
        end
    end
    
    return isArray and #continuousArray > 0, continuousArray, mapPart
end

-- 便捷方法：打印到控制台
function Dump.print(value, options)
    options = options or {}
    options.pretty = true
    print(Dump.dump(value, options))
end

-- 便捷方法：返回字符串（不美化）
function Dump.toString(value, options)
    options = options or {}
    options.pretty = false
    return Dump.dump(value, options)
end

-- 便捷方法：输出到文件
function Dump.toFile(filename, value, options)
    local file = io.open(filename, "w")
    if not file then
        return false, "无法打开文件"
    end
    
    options = options or {}
    options.pretty = true
    file:write(Dump.dump(value, options))
    file:close()
    return true
end

-- 返回模块
return Dump