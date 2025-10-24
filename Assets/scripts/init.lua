function class(__ClassName, __Base)
    local cls = {}
    cls.__clsname = __ClassName
    cls.__index = cls

    local baseType = type(__Base)
    if baseType == "table" then
        cls.__super = __Base
        setmetatable(cls, {
            __index = __Base,
        })
    elseif baseType == "function" then
        function cls.new(...)
            local userdata = __Base(...)
            cls.__super = userdata
            setmetatable(cls, {
                __index = function (table, key)
                    local ok, v = pcall(function () return userdata[key] end)
                    if ok then
                        if type(v) == "function" then
                            return function (_, ...)
                                return v(...)
                            end
                        else
                            return v
                        end
                    end
                    return nil
                end,
                __newindex = function (table, key, value)
                    local ok, _ = pcall(function () userdata[key] = value end)
                    if ok then
                        return
                    end
                    rawset(table, key, value)
                end,
            })
            local instance = setmetatable({}, cls)
            if instance.ctor then
                instance:ctor(...)
            end
            return instance
        end
    end

    if cls.new == nil then
        cls.new = function (...)
            local instance = setmetatable({}, cls)
            if instance.ctor then
                instance:ctor(...)
            end
            return instance
        end
    end

    return cls
end

function handler(target, selector, ...)
	local args = {...}
	return function(...)
		-- the varadic params here are sent by the event automatically, append our own args after them.
		local internalArgs = {...}
		for _, arg in ipairs(args) do
			table.insert(internalArgs, arg)
		end
		return selector(target, unpack(internalArgs))
	end
end