local MathsHelper = {}

function MathsHelper.addVec2(vec1, vec2)
    return {
        x = vec1.x + vec2.x,
        y = vec1.y + vec2.y,
    }
end

function MathsHelper.subVec2(vec1, vec2)
    return {
        x = vec1.x - vec2.x,
        y = vec1.y - vec2.y,
    }
end

function MathsHelper.mulVec2(vec, scalar)
    return {
        x = vec.x * scalar,
        y = vec.y * scalar,
    }
end

function MathsHelper.divVec2(vec, scalar)
    return {
        x = vec.x / scalar,
        y = vec.y / scalar,
    }
end

function MathsHelper.lengthOfVec2(vec)
    return math.sqrt(vec.x * vec.x + vec.y * vec.y)
end

function MathsHelper.distanceOfVec2(vec1, vec2)
    local dx = vec2.x - vec1.x
    local dy = vec2.y - vec1.y
    return math.sqrt(dx * dx + dy * dy)
end

function MathsHelper.radianOfVec2(from, to)
    if to == nil then
        to = from
        from = { x = 0.0, y = -1.0, }
    end
    -- 编辑器定义的旋转角度是顺时针为正，所以这里取负值
    return -math.atan2(to.y - from.y, to.x - from.x)
end

return MathsHelper